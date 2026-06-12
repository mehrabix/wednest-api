namespace WedNest.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// PostgreSQL optimistic concurrency token (maps to xmin system column).
    /// Auto-incremented by PostgreSQL on every UPDATE. Do not set manually.
    /// </summary>
    public uint xmin { get; set; }
}
