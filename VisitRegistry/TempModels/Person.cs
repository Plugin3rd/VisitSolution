using System;
using System.Collections.Generic;

namespace VisitRegistry.TempModels;

public partial class Person
{
    public int PersonId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string NationalCode { get; set; } = null!;

    public string Mobile { get; set; } = null!;

    public int IsActive { get; set; }

    public int OrganizationalUnitId { get; set; }

    public string? CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? LastModifiedByUserId { get; set; }

    public string? LastModifiedAt { get; set; }

    public byte[] RecordVersion { get; set; } = null!;

    public virtual ICollection<BlackList> BlackLists { get; set; } = new List<BlackList>();

    public virtual OrganizationalUnit OrganizationalUnit { get; set; } = null!;

    public virtual ICollection<Visit> Visits { get; set; } = new List<Visit>();
}
