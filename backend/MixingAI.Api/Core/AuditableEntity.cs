namespace MixingAI.Api.Core;

public abstract class AuditableEntity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public Guid? CreatedByUserId { get; private set; }
    public DateTime? UpdatedAtUtc { get; private set; }
    public Guid? UpdatedByUserId { get; private set; }

    public void SetCreated(Guid? userId, DateTime utcNow)
    {
        CreatedAtUtc = utcNow;
        CreatedByUserId = userId;
    }

    public void SetUpdated(Guid? userId, DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
        UpdatedByUserId = userId;
    }
}
