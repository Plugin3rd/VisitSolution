using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.Models;

public class Visit : AuditableBase
{
    public long VisitId { get; set; }
    public int PersonId { get; set; }
    // Loaded by EF after binding; it is not posted by the create/exit forms.
    public Person? Person { get; set; }

    [Display(Name = "تاریخ ورود")]
    public DateTime EntryDate { get; set; }

    [Display(Name = "ساعت ورود")]
    public TimeSpan EntryTime { get; set; }

    [MaxLength(1000)]
    [Display(Name = "توضیحات ورود")]
    public string? EntryDescription { get; set; }

    [Display(Name = "واحد مراجعه‌کننده")]
    public int VisitingUnitId { get; set; }
    // Loaded by EF after binding; the form posts only VisitingUnitId.
    public OrganizationalUnit? VisitingUnit { get; set; }

    [Display(Name = "تاریخ خروج")]
    public DateTime? ExitDate { get; set; }

    [Display(Name = "ساعت خروج")]
    public TimeSpan? ExitTime { get; set; }

    [MaxLength(1000)]
    [Display(Name = "توضیحات خروج")]
    public string? ExitDescription { get; set; }

    public bool IsOpen => ExitDate is null || ExitTime is null;
}
