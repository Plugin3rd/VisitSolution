using System;
using System.Collections.Generic;

namespace VisitRegistry.TempModels;

public partial class OrganizationalUnit
{
    public int OrganizationalUnitId { get; set; }

    public string Title { get; set; } = null!;

    public string? Code { get; set; }

    public int IsActive { get; set; }

    public string? Description { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastModifiedByUserId { get; set; }

    public string? LastModifiedAt { get; set; }

    public byte[] RecordVersion { get; set; } = null!;

    public virtual ICollection<Person> People { get; set; } = new List<Person>();

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
