using System;
using System.Collections.Generic;

namespace VisitRegistry.TempModels;

public partial class AuditLog
{
    public int AuditLogId { get; set; }

    public string? UserId { get; set; }

    public string EntityName { get; set; } = null!;

    public string EntityId { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? OldValues { get; set; }

    public string? NewValues { get; set; }

    public string? IpAddress { get; set; }

    public DateTime CreatedAt { get; set; }
}
