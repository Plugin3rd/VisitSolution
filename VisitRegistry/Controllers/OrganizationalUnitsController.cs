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
    public async Task<IActionResult> Index() => View(await db.OrganizationalUnits.AsNoTracking().OrderBy(x => x.Title).ToListAsync());

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
}
