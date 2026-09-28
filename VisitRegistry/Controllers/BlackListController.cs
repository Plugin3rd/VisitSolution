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
        ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
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

        if (!ModelState.IsValid)
        {
            ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
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
}
