using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.ViewModels;

public class UserCreateViewModel
{
    [Required(ErrorMessage = "نام کاربری الزامی است.")]
    [Display(Name = "نام کاربری")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام نمایشی الزامی است.")]
    [Display(Name = "نام نمایشی")]
    public string DisplayName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رمز عبور الزامی است.")]
    [MinLength(8, ErrorMessage = "رمز عبور باید حداقل ۸ کاراکتر باشد.")]
    [DataType(DataType.Password)]
    [Display(Name = "رمز عبور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "انتخاب نقش الزامی است.")]
    [Display(Name = "نقش")]
    public string Role { get; set; } = string.Empty;
}
