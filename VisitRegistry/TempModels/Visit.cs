using System;
using System.Collections.Generic;

namespace VisitRegistry.TempModels;

public partial class Visit
{
    public int VisitId { get; set; }

    public int PersonId { get; set; }

    public DateTime EntryDate { get; set; }

    public TimeSpan EntryTime { get; set; }

    public string? EntryDescription { get; set; }

    public int VisitingUnitId { get; set; }

    public DateTime? ExitDate { get; set; }

    public TimeSpan? ExitTime { get; set; }

    public string? ExitDescription { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastModifiedByUserId { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public byte[] RecordVersion { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;

    public virtual OrganizationalUnit VisitingUnit { get; set; } = null!;
}
