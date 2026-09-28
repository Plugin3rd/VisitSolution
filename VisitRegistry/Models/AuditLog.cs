using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.Models;

public class AuditLog
{
    public long AuditLogId { get; set; }
    public string? UserId { get; set; }
    [MaxLength(100)] public string EntityName { get; set; } = string.Empty;
    [MaxLength(100)] public string EntityId { get; set; } = string.Empty;
    [MaxLength(50)] public string Action { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    [MaxLength(50)] public string? IpAddress { get; set; }
    public DateTime CreatedAt { get; set; }
}
