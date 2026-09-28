using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.Models;
using VisitRegistry.Services;
using VisitRegistry.ViewModels;

namespace VisitRegistry.Controllers;

[Authorize]
public class BlackListController(ApplicationDbContext db, ICurrentUserService currentUser, AuditService audit, PersianDateService persianDate) : Controller
{
    public async Task<IActionResult> Index() => View(await db.BlackLists.Include(x => x.Person).AsNoTracking().OrderByDescending(x => x.StartDate).ToListAsync());

    [Authorize(Roles = "مدیر سامانه")]
    public async Task<IActionResult> Create()
    {
        await LoadPersonsAsync();
        return View(new BlackListCreateViewModel { StartDate = persianDate.ToPersian(DateTime.Today), IsActive = true });
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BlackListCreateViewModel model)
    {
        if (!persianDate.TryParse(model.StartDate, out var start))
            ModelState.AddModelError(nameof(model.StartDate), "تاریخ شروع را به صورت ۱۴۰۵/۰۷/۰۶ وارد کنید.");

        DateTime? end = null;
        if (!string.IsNullOrWhiteSpace(model.EndDate))
        {
            if (!persianDate.TryParse(model.EndDate, out var parsedEnd))
                ModelState.AddModelError(nameof(model.EndDate), "تاریخ پایان معتبر نیست.");
            else end = parsedEnd;
        }

        if (ModelState.IsValid && end.HasValue && end.Value.Date < start.Date)
            ModelState.AddModelError(nameof(model.EndDate), "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");

        if (!await db.Persons.AnyAsync(x => x.PersonId == model.PersonId))
            ModelState.AddModelError(nameof(model.PersonId), "شخص انتخاب‌شده وجود ندارد.");

        if (!ModelState.IsValid)
        {
            await LoadPersonsAsync();
            return View(model);
        }

        var entity = new BlackListEntry
        {
            PersonId = model.PersonId,
            Reason = model.Reason,
            StartDate = start,
            EndDate = end,
            IsActive = model.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = currentUser.UserId
        };
        db.BlackLists.Add(entity);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(BlackListEntry), entity.BlackListEntryId.ToString(), "ایجاد", null, new { entity.PersonId, entity.Reason });
        TempData["Success"] = "رکورد بلک‌لیست با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "مدیر سامانه")]
    public async Task<IActionResult> Edit(long id)
    {
        var entity = await db.BlackLists.AsNoTracking().FirstOrDefaultAsync(x => x.BlackListEntryId == id);
        if (entity is null) return NotFound();
        await LoadPersonsAsync();
        return View(new BlackListEditViewModel
        {
            BlackListEntryId = entity.BlackListEntryId,
            PersonId = entity.PersonId,
            Reason = entity.Reason,
            StartDate = persianDate.ToPersian(entity.StartDate),
            EndDate = entity.EndDate.HasValue ? persianDate.ToPersian(entity.EndDate.Value) : null,
            IsActive = entity.IsActive,
            RecordVersion = Convert.ToBase64String(entity.RecordVersion)
        });
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, BlackListEditViewModel model)
    {
        if (id != model.BlackListEntryId) return BadRequest();
        var entity = await db.BlackLists.FirstOrDefaultAsync(x => x.BlackListEntryId == id);
        if (entity is null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.RecordVersion) || Convert.ToBase64String(entity.RecordVersion) != model.RecordVersion)
            ModelState.AddModelError(string.Empty, "این اطلاعات توسط کاربر دیگری تغییر کرده است. صفحه را مجدداً بارگذاری کنید.");

        if (!persianDate.TryParse(model.StartDate, out var start))
            ModelState.AddModelError(nameof(model.StartDate), "تاریخ شروع را به صورت ۱۴۰۵/۰۷/۰۶ وارد کنید.");

        DateTime? end = null;
        if (!string.IsNullOrWhiteSpace(model.EndDate))
        {
            if (!persianDate.TryParse(model.EndDate, out var parsedEnd))
                ModelState.AddModelError(nameof(model.EndDate), "تاریخ پایان معتبر نیست.");
            else end = parsedEnd;
        }

        if (ModelState.IsValid && end.HasValue && end.Value.Date < start.Date)
            ModelState.AddModelError(nameof(model.EndDate), "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد.");

        if (!await db.Persons.AnyAsync(x => x.PersonId == model.PersonId))
            ModelState.AddModelError(nameof(model.PersonId), "شخص انتخاب‌شده وجود ندارد.");

        if (!ModelState.IsValid)
        {
            await LoadPersonsAsync();
            return View(model);
        }

        var oldValues = new { entity.PersonId, entity.Reason, entity.StartDate, entity.EndDate, entity.IsActive };
        entity.PersonId = model.PersonId;
        entity.Reason = model.Reason;
        entity.StartDate = start;
        entity.EndDate = end;
        entity.IsActive = model.IsActive;
        entity.LastModifiedByUserId = currentUser.UserId;
        entity.LastModifiedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(BlackListEntry), id.ToString(), "ویرایش", oldValues, new { entity.PersonId, entity.Reason, entity.StartDate, entity.EndDate, entity.IsActive });
        TempData["Success"] = "رکورد بلک‌لیست با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var entity = await db.BlackLists.FirstOrDefaultAsync(x => x.BlackListEntryId == id);
        if (entity is null) return NotFound();
        db.BlackLists.Remove(entity);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(BlackListEntry), id.ToString(), "حذف", new { entity.PersonId, entity.Reason, entity.StartDate, entity.EndDate, entity.IsActive }, null);
        TempData["Success"] = "رکورد بلک‌لیست با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadPersonsAsync() =>
        ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
}
