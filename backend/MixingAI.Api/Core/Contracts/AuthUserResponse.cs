namespace MixingAI.Api.Core.Contracts;

public sealed record AuthUserResponse(
    Guid UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool IsAdmin);

public sealed record LoginResponse(
    string Token,
    DateTime ExpiresAtUtc,
    AuthUserResponse User);
