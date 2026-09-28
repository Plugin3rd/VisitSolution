namespace VisitRegistry.Models;

public abstract class AuditableBase
{
    public string? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? LastModifiedByUserId { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public byte[] RecordVersion { get; set; } = Array.Empty<byte>();
}
