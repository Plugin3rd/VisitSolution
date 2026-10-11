using System.ComponentModel.DataAnnotations;

namespace VisitRegistry.Models;

public enum UnitType
{
    [Display(Name = "واحد فعالیت")]
    ActivityUnit = 1,
    
    [Display(Name = "واحد مراجعه")]
    VisitingUnit = 2
}

public class OrganizationalUnit : AuditableBase
{
    public int OrganizationalUnitId { get; set; }

    [Required(ErrorMessage = "عنوان واحد الزامی است.")]
    [MaxLength(200)]
    [Display(Name = "عنوان واحد")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(50)]
    [Display(Name = "کد واحد")]
    public string? Code { get; set; }

    [Display(Name = "نوع واحد")]
    [Required(ErrorMessage = "نوع واحد الزامی است.")]
    public UnitType UnitType { get; set; } = UnitType.ActivityUnit;

    [Display(Name = "فعال")]
    public bool IsActive { get; set; } = true;

    [MaxLength(1000)]
    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    public ICollection<Person> Persons { get; set; } = [];
    public ICollection<Visit> Visits { get; set; } = [];
}
