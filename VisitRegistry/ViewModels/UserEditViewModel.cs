using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.ViewModels;

public class UserEditViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نمایشی الزامی است.")]
    [Display(Name = "نام نمایشی")]
    public string DisplayName { get; set; } = string.Empty;

    [Display(Name = "رمز عبور جدید")]
    [MinLength(8, ErrorMessage = "رمز عبور باید حداقل ۸ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [Required(ErrorMessage = "انتخاب نقش الزامی است.")]
    [Display(Name = "نقش")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;

    public string? RecordVersion { get; set; }
}
