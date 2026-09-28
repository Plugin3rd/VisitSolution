using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.ViewModels;

public class VisitEntryViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "انتخاب شخص الزامی است.")]
    public int PersonId { get; set; }

    [Required(ErrorMessage = "انتخاب واحد مراجعه‌کننده الزامی است.")]
    public int VisitingUnitId { get; set; }

    [Required(ErrorMessage = "ساعت ورود الزامی است.")]
    public TimeSpan EntryTime { get; set; } = DateTime.Now.TimeOfDay;

    [MaxLength(1000)]
    public string? EntryDescription { get; set; }
}
