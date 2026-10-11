using System;
using System.Collections.Generic;

namespace VisitRegistry.TempModels;

public partial class BlackList
{
    public int BlackListEntryId { get; set; }

    public int PersonId { get; set; }

    public string Reason { get; set; } = null!;

    public int IsActive { get; set; }

    public string StartDate { get; set; } = null!;

    public string? EndDate { get; set; }

    public string? CreatedByUserId { get; set; }

    public string CreatedAt { get; set; } = null!;

    public string? LastModifiedByUserId { get; set; }

    public string? LastModifiedAt { get; set; }

    public byte[] RecordVersion { get; set; } = null!;

    public virtual Person Person { get; set; } = null!;
}
