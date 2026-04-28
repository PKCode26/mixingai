using System.Security.Cryptography;

namespace MixingAI.Api.Core.Services;

public sealed class StorageService
{
    private readonly string _root;

    public StorageService(IConfiguration config, IHostEnvironment env)
    {
        var configured = config.GetValue<string>("Storage:DocumentRootPath") ?? "storage/documents";
        _root = Path.IsPathRooted(configured)
            ? configured
            : Path.GetFullPath(configured, env.ContentRootPath);
        Directory.CreateDirectory(_root);
    }

    public static string ComputeHash(Stream stream)
    {
        var bytes = SHA256.HashData(stream);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public async Task<string> SaveAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var ext = Path.GetExtension(file.FileName);
        var now = DateTime.UtcNow;
        var relative = Path.Combine(now.Year.ToString(), now.Month.ToString("D2"), $"{Guid.NewGuid()}{ext}");
        var full = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        await using var dest = File.Create(full);
        await file.CopyToAsync(dest, cancellationToken);
        return relative;
    }

    public FileStream OpenRead(string relativePath) =>
        File.OpenRead(Path.Combine(_root, relativePath));

    public void Delete(string relativePath)
    {
        var full = Path.Combine(_root, relativePath);
        if (File.Exists(full))
            File.Delete(full);
    }
}
