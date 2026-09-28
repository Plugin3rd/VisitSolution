using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.ViewModels;

public class BlackListEditViewModel
{
    public long BlackListEntryId { get; set; }

    [Required(ErrorMessage = "انتخاب شخص الزامی است.")]
    public int PersonId { get; set; }

    [Required(ErrorMessage = "علت ممنوعیت الزامی است.")]
    [MaxLength(1000)]
    [Display(Name = "علت ممنوعیت")]
    public string Reason { get; set; } = string.Empty;

    [Required(ErrorMessage = "تاریخ شروع الزامی است.")]
    [Display(Name = "تاریخ شروع")]
    public string StartDate { get; set; } = string.Empty;

    [Display(Name = "تاریخ پایان")]
    public string? EndDate { get; set; }

    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;

    public string? RecordVersion { get; set; }
}
