using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.Models;
using VisitRegistry.Services;

namespace VisitRegistry.Controllers;

[Authorize]
public class OrganizationalUnitsController(ApplicationDbContext db, ICurrentUserService currentUser, AuditService audit) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        search = search?.Trim();

        var query = db.OrganizationalUnits.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.Title.Contains(search) ||
                (x.Code != null && x.Code.Contains(search)) ||
                (x.Description != null && x.Description.Contains(search)));
        }

        var units = await query
            .OrderBy(x => x.Title)
            .ToListAsync();

        return View(units);
    }

    [Authorize(Roles = "مدیر سامانه")]
    public IActionResult Create() => View(new OrganizationalUnit());

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationalUnit model)
    {
        if (await db.OrganizationalUnits.AnyAsync(x => x.Title == model.Title))
            ModelState.AddModelError(nameof(model.Title), "این واحد قبلاً ثبت شده است.");
        if (!ModelState.IsValid) return View(model);
        model.CreatedAt = DateTime.UtcNow;
        model.CreatedByUserId = currentUser.UserId;
        db.OrganizationalUnits.Add(model);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(OrganizationalUnit), model.OrganizationalUnitId.ToString(), "ایجاد", null, new { model.Title });
        TempData["Success"] = "واحد با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "مدیر سامانه")]
    public async Task<IActionResult> Edit(int id)
    {
        var unit = await db.OrganizationalUnits.AsNoTracking().FirstOrDefaultAsync(x => x.OrganizationalUnitId == id);
        return unit is null ? NotFound() : View(unit);
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, OrganizationalUnit model)
    {
        if (id != model.OrganizationalUnitId) return BadRequest();
        var entity = await db.OrganizationalUnits.FirstOrDefaultAsync(x => x.OrganizationalUnitId == id);
        if (entity is null) return NotFound();

        if (model.RecordVersion is null || !model.RecordVersion.SequenceEqual(entity.RecordVersion))
            ModelState.AddModelError(string.Empty, "این اطلاعات توسط کاربر دیگری تغییر کرده است. صفحه را مجدداً بارگذاری کنید.");

        if (await db.OrganizationalUnits.AnyAsync(x => x.OrganizationalUnitId != id && x.Title == model.Title))
            ModelState.AddModelError(nameof(model.Title), "این عنوان قبلاً برای واحد دیگری ثبت شده است.");

        if (!ModelState.IsValid) return View(model);

        var oldValues = new { entity.Title, entity.Code, entity.IsActive, entity.Description };
        entity.Title = model.Title;
        entity.Code = model.Code;
        entity.IsActive = model.IsActive;
        entity.Description = model.Description;
        entity.LastModifiedByUserId = currentUser.UserId;
        entity.LastModifiedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(OrganizationalUnit), id.ToString(), "ویرایش", oldValues, new { entity.Title, entity.Code, entity.IsActive, entity.Description });
        TempData["Success"] = "واحد با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.OrganizationalUnits.FirstOrDefaultAsync(x => x.OrganizationalUnitId == id);
        if (entity is null) return NotFound();

        var hasPersons = await db.Persons.AnyAsync(x => x.OrganizationalUnitId == id);
        var hasVisits = await db.Visits.AnyAsync(x => x.VisitingUnitId == id);
        if (hasPersons || hasVisits)
        {
            TempData["Error"] = "امکان حذف این واحد وجود ندارد، زیرا در اطلاعات اشخاص یا سوابق مراجعات استفاده شده است. در صورت نیاز آن را غیرفعال کنید.";
            return RedirectToAction(nameof(Index));
        }

        db.OrganizationalUnits.Remove(entity);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(OrganizationalUnit), id.ToString(), "حذف", new { entity.Title, entity.Code }, null);
        TempData["Success"] = "واحد با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index));
    }
}
