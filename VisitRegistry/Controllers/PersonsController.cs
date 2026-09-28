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
        ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
        return View(new Person());
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Person model)
    {
        if (await db.Persons.AnyAsync(x => x.NationalCode == model.NationalCode))
            ModelState.AddModelError(nameof(model.NationalCode), "این شماره ملی قبلاً ثبت شده است.");

        if (!ModelState.IsValid)
        {
            ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
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
}
