using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace MixingAI.Api.Core.Ai;

public sealed class OllamaService : IOllamaService
{
    private const string SystemPrompt =
        """
        You are a data extraction assistant for an industrial mixing technology company.
        Extract technical parameters from documents and return them as a FLAT JSON object.

        CRITICAL RULES:
        1. Return ONLY raw JSON — no markdown, no code blocks, no backticks, no explanations.
        2. The JSON must be completely FLAT — no nested objects, no arrays.
        3. Keys: short descriptive names (German preferred). Use underscores for spaces.
        4. Values: always strings, include units, e.g. "350 kg", "8 min", "120 rpm".
        5. For multiple components use: "Komponente_1", "Anteil_1", "Komponente_2", "Anteil_2"...
        6. If nothing is found, return exactly: {}

        CORRECT: {"Zement": "350 kg", "Mischzeit": "8 min", "Drehzahl": "120 rpm"}
        WRONG:   {"Mix": {"Zement": "350 kg"}} or ```json{...}```
        """;

    private const string UserPromptTemplate =
        """
        Extract all technical parameters from this document:

        ---
        {0}
        ---
        """;

    private const int MaxTextLength = 8000;

    private readonly HttpClient _http;
    private readonly ILogger<OllamaService> _logger;

    public bool IsAvailable { get; }
    public string ModelName { get; }

    public OllamaService(IConfiguration config, IHttpClientFactory httpFactory, ILogger<OllamaService> logger)
    {
        _logger = logger;
        _http   = httpFactory.CreateClient("ollama");

        var section  = config.GetSection("Ollama");
        var baseUrl  = section["BaseUrl"] ?? "http://localhost:11434";
        ModelName    = section["Model"] ?? "llama3.2:3b";
        IsAvailable  = !string.IsNullOrWhiteSpace(baseUrl);

        _http.BaseAddress = new Uri(baseUrl);
        _http.Timeout     = TimeSpan.FromSeconds(
            section.GetValue("TimeoutSeconds", 120));
    }

    public async Task<OllamaExtractionResult> ExtractAsync(string documentText, CancellationToken ct)
    {
        var text    = documentText.Length > MaxTextLength
            ? documentText[..MaxTextLength]
            : documentText;
        var prompt  = string.Format(UserPromptTemplate, text);

        var body = new
        {
            model  = ModelName,
            stream = false,
            messages = new[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user",   content = prompt },
            },
            options = new { temperature = 0.1 },
        };

        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("/api/chat", body, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama HTTP-Fehler");
            return new OllamaExtractionResult(false, $"Ollama nicht erreichbar: {ex.Message}", EmptyFields, null);
        }

        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("Ollama antwortete {Status}: {Body}", response.StatusCode, err);
            return new OllamaExtractionResult(false, $"Ollama Fehler {(int)response.StatusCode}: {err}", EmptyFields, null);
        }

        var raw = await response.Content.ReadAsStringAsync(ct);
        var content = ExtractMessageContent(raw);

        if (content is null)
            return new OllamaExtractionResult(false, "Ollama-Antwort konnte nicht geparst werden.", EmptyFields, raw);

        var fields = ParseJsonFields(content);
        if (fields is null)
        {
            _logger.LogWarning("JSON aus Ollama-Antwort nicht parsebar: {Content}", content[..Math.Min(content.Length, 500)]);
            return new OllamaExtractionResult(false, "Ollama-Antwort enthielt kein gültiges JSON.", EmptyFields, content);
        }

        _logger.LogInformation("Ollama extrahierte {Count} Felder.", fields.Count);
        return new OllamaExtractionResult(true, null, fields, content);
    }

    private static string? ExtractMessageContent(string ollamaJson)
    {
        try
        {
            var doc = JsonNode.Parse(ollamaJson);
            return doc?["message"]?["content"]?.GetValue<string>();
        }
        catch { return null; }
    }

    private static IReadOnlyDictionary<string, string>? ParseJsonFields(string text)
    {
        // 1. Try direct parse
        var result = TryParseObject(text.Trim());
        if (result is not null) return result;

        // 2. Extract from markdown code block
        var md = Regex.Match(text, @"```(?:json)?\s*(\{[\s\S]*?\})\s*```");
        if (md.Success)
        {
            result = TryParseObject(md.Groups[1].Value);
            if (result is not null) return result;
        }

        // 3. Find first { ... } block
        var start = text.IndexOf('{');
        var end   = text.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            result = TryParseObject(text[start..(end + 1)]);
            if (result is not null) return result;
        }

        return null;
    }

    private static Dictionary<string, string>? TryParseObject(string json)
    {
        try
        {
            var node = JsonNode.Parse(json);
            if (node is not JsonObject obj) return null;

            var dict = new Dictionary<string, string>(StringComparer.Ordinal);
            FlattenInto(obj, dict, "");
            return dict;
        }
        catch { return null; }
    }

    private static void FlattenInto(JsonObject obj, Dictionary<string, string> result, string prefix)
    {
        foreach (var kv in obj)
        {
            if (string.IsNullOrWhiteSpace(kv.Key)) continue;
            var key = prefix.Length > 0 ? $"{prefix}_{kv.Key}" : kv.Key;

            switch (kv.Value)
            {
                case JsonObject nested:
                    FlattenInto(nested, result, key);
                    break;
                case JsonArray arr:
                    for (var i = 0; i < arr.Count; i++)
                    {
                        var item = arr[i];
                        if (item is JsonObject arrObj)
                            FlattenInto(arrObj, result, $"{key}_{i + 1}");
                        else
                        {
                            var v = item?.ToString();
                            if (!string.IsNullOrWhiteSpace(v))
                                result[$"{key}_{i + 1}"] = v;
                        }
                    }
                    break;
                default:
                    var val = kv.Value?.GetValue<object>()?.ToString();
                    if (!string.IsNullOrWhiteSpace(val))
                        result[key] = val;
                    break;
            }
        }
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyFields =
        new Dictionary<string, string>();
}
