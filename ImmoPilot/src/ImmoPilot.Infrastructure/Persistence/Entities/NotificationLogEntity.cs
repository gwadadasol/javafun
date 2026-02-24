namespace ImmoPilot.Infrastructure.Persistence.Entities;

public class NotificationLogEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PropertyId { get; set; }
    public PropertyEntity Property { get; set; } = null!;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public DateOnly DigestDate { get; set; }
    public required string Recipients { get; set; }
}
