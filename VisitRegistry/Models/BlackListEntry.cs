using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.Models;

public class BlackListEntry : AuditableBase
{
    public long BlackListEntryId { get; set; }
    public int PersonId { get; set; }
    public Person Person { get; set; } = null!;

    [Required(ErrorMessage = "علت ممنوعیت الزامی است.")]
    [MaxLength(1000)]
    [Display(Name = "علت ممنوعیت")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "تاریخ شروع")]
    public DateTime StartDate { get; set; }

    [Display(Name = "تاریخ پایان")]
    public DateTime? EndDate { get; set; }
}
