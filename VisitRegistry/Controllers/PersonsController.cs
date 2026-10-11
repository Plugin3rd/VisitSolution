using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.Models;
using VisitRegistry.Services;

namespace VisitRegistry.Controllers;

[Authorize]
public class PersonsController(ApplicationDbContext db, ICurrentUserService currentUser, AuditService audit) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Persons.Include(x => x.OrganizationalUnit).AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.FirstName.Contains(search) || x.LastName.Contains(search) || x.NationalCode.Contains(search) || x.Mobile.Contains(search));
        return View(await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync());
    }

    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    public async Task<IActionResult> Create()
    {
        await LoadUnitsAsync();
        return View(new Person());
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person model)
    {
        model.NationalCode = NormalizeNationalCode(model.NationalCode);
        if (!IsValidNationalCode(model.NationalCode))
            ModelState.AddModelError(nameof(model.NationalCode), "شماره ملی معتبر نیست.");

        if (await db.Persons.AnyAsync(x => x.NationalCode == model.NationalCode))
            ModelState.AddModelError(nameof(model.NationalCode), "این شماره ملی قبلاً ثبت شده است.");

        if (!await db.OrganizationalUnits.AnyAsync(x => x.OrganizationalUnitId == model.OrganizationalUnitId && x.IsActive))
            ModelState.AddModelError(nameof(model.OrganizationalUnitId), "واحد سازمانی انتخاب‌شده معتبر نیست.");

        if (!ModelState.IsValid)
        {
            await LoadUnitsAsync();
            return View(model);
        }

        model.CreatedByUserId = currentUser.UserId;
        model.CreatedAt = DateTime.UtcNow;
        db.Persons.Add(model);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(Person), model.PersonId.ToString(), "ایجاد", null, new { model.FirstName, model.LastName, model.NationalCode });
        TempData["Success"] = "شخص با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    public async Task<IActionResult> Edit(int id)
    {
        var person = await db.Persons.AsNoTracking().FirstOrDefaultAsync(x => x.PersonId == id);
        if (person is null) return NotFound();
        await LoadUnitsAsync();
        return View(person);
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Person model)
    {
        if (id != model.PersonId) return BadRequest();

        var entity = await db.Persons.FirstOrDefaultAsync(x => x.PersonId == id);
        if (entity is null) return NotFound();

        if (model.RecordVersion is null || !model.RecordVersion.SequenceEqual(entity.RecordVersion))
        {
            ModelState.AddModelError(string.Empty, "این اطلاعات توسط کاربر دیگری تغییر کرده است. صفحه را مجدداً بارگذاری کنید.");
        }

        if (await db.Persons.AnyAsync(x => x.PersonId != id && x.NationalCode == model.NationalCode))
            ModelState.AddModelError(nameof(model.NationalCode), "این شماره ملی قبلاً برای شخص دیگری ثبت شده است.");

        if (!await db.OrganizationalUnits.AnyAsync(x => x.OrganizationalUnitId == model.OrganizationalUnitId && x.IsActive))
            ModelState.AddModelError(nameof(model.OrganizationalUnitId), "واحد سازمانی انتخاب‌شده معتبر نیست.");

        if (!ModelState.IsValid)
        {
            await LoadUnitsAsync();
            return View(model);
        }

        var oldValues = new { entity.FirstName, entity.LastName, entity.NationalCode, entity.Mobile, entity.IsActive, entity.OrganizationalUnitId };
        entity.FirstName = model.FirstName;
        entity.LastName = model.LastName;
        entity.NationalCode = model.NationalCode;
        entity.Mobile = model.Mobile;
        entity.IsActive = model.IsActive;
        entity.OrganizationalUnitId = model.OrganizationalUnitId;
        entity.LastModifiedByUserId = currentUser.UserId;
        entity.LastModifiedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(Person), entity.PersonId.ToString(), "ویرایش", oldValues, new { entity.FirstName, entity.LastName, entity.NationalCode, entity.Mobile, entity.IsActive, entity.OrganizationalUnitId });
        TempData["Success"] = "اطلاعات شخص با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Persons.FirstOrDefaultAsync(x => x.PersonId == id);
        if (entity is null) return NotFound();

        var hasVisits = await db.Visits.AnyAsync(x => x.PersonId == id);
        var hasBlackList = await db.BlackLists.AnyAsync(x => x.PersonId == id);
        if (hasVisits || hasBlackList)
        {
            TempData["Error"] = "امکان حذف این شخص وجود ندارد، زیرا برای او سابقه مراجعه یا رکورد بلک‌لیست ثبت شده است. در صورت نیاز وضعیت شخص را غیرفعال کنید.";
            return RedirectToAction(nameof(Index));
        }

        db.Persons.Remove(entity);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(Person), id.ToString(), "حذف", new { entity.FirstName, entity.LastName, entity.NationalCode }, null);
        TempData["Success"] = "شخص با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadUnitsAsync() =>
        ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive && x.UnitType == UnitType.ActivityUnit).OrderBy(x => x.Title).ToListAsync();

    private static string NormalizeNationalCode(string? nationalCode) =>
        (nationalCode ?? string.Empty)
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
            .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
            .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');

    private static bool IsValidNationalCode(string nationalCode)
    {
        if (nationalCode.Length != 10 || nationalCode.Any(character => character is < '0' or > '9'))
            return false;

        if (nationalCode.All(character => character == nationalCode[0]))
            return false;

        var sum = 0;
        for (var index = 0; index < 9; index++)
            sum += (nationalCode[index] - '0') * (10 - index);

        var remainder = sum % 11;
        var checkDigit = remainder < 2 ? remainder : 11 - remainder;
        return nationalCode[9] - '0' == checkDigit;
    }
}
