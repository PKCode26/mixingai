namespace MixingAI.Api.Core.Contracts;

public sealed record LoginRequest(string UsernameOrEmail, string Password);
