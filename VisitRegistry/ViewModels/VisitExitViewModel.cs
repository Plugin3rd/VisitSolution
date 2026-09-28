using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.ViewModels;

public class VisitExitViewModel
{
    public long VisitId { get; set; }

    [Required(ErrorMessage = "ساعت خروج الزامی است.")]
    public TimeSpan ExitTime { get; set; } = DateTime.Now.TimeOfDay;

    [MaxLength(1000)]
    public string? ExitDescription { get; set; }
}
