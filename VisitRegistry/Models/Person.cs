using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.Models;

public class Person : AuditableBase
{
    public int PersonId { get; set; }

    [Required(ErrorMessage = "نام الزامی است.")]
    [MaxLength(100)]
    [Display(Name = "نام")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "نام خانوادگی الزامی است.")]
    [MaxLength(100)]
    [Display(Name = "نام خانوادگی")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره ملی الزامی است.")]
    [StringLength(10, MinimumLength = 10, ErrorMessage = "شماره ملی باید ۱۰ رقم باشد.")]
    [Display(Name = "شماره ملی")]
    public string NationalCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "شماره موبایل الزامی است.")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "شماره موبایل باید ۱۱ رقم باشد.")]
    [Display(Name = "موبایل")]
    public string Mobile { get; set; } = string.Empty;

    [Display(Name = "وضعیت")]
    public bool IsActive { get; set; } = true;

    [Display(Name = "واحد سازمانی")]
    public int OrganizationalUnitId { get; set; }
    public OrganizationalUnit OrganizationalUnit { get; set; } = null!;

    public ICollection<Visit> Visits { get; set; } = [];
    public ICollection<BlackListEntry> BlackListEntries { get; set; } = [];
}
