using MixingAI.Api.Core.Users;
using Microsoft.AspNetCore.Identity;

namespace MixingAI.Api.Core.Security;

public sealed class PasswordHashingService
{
    private readonly PasswordHasher<User> _hasher = new();

    public const string DevAdminPassword = "Admin123!";

    public string HashPassword(User user, string password) =>
        _hasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string password) =>
        _hasher.VerifyHashedPassword(user, user.PasswordHash, password)
            is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
