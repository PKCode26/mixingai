namespace MixingAI.Api.Core.Users;

public sealed class User : AuditableEntity
{
    public required string Username { get; init; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public bool IsAdmin { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime? LockedUntilUtc { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }

    public void UpdateProfile(string email, string firstName, string lastName)
    {
        Email = email;
        FirstName = firstName;
        LastName = lastName;
    }

    public void ChangePassword(string passwordHash) => PasswordHash = passwordHash;

    public void SetAdmin(bool isAdmin) => IsAdmin = isAdmin;

    public void RegisterLogin(DateTime utcNow) => LastLoginAtUtc = utcNow;

    public void LockUntil(DateTime utcUntil) => LockedUntilUtc = utcUntil;

    public void Unlock() => LockedUntilUtc = null;

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
