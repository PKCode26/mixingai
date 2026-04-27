namespace MixingAI.Api.Core.Security;

public sealed record RequestUserContext(
    Guid UserId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    bool IsAdmin,
    bool IsActive);
