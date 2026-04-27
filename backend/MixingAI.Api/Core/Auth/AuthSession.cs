namespace MixingAI.Api.Core.Auth;

public sealed class AuthSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid UserId { get; init; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;

    public void Initialize(string tokenHash, DateTime expiresAtUtc, DateTime utcNow)
    {
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = utcNow;
    }

    public void Revoke(DateTime utcNow) => RevokedAtUtc = utcNow;

    public bool IsActive(DateTime utcNow) =>
        RevokedAtUtc is null && ExpiresAtUtc > utcNow;
}
