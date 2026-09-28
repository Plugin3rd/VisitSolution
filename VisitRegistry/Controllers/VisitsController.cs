using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.Services;
using VisitRegistry.ViewModels;

namespace VisitRegistry.Controllers;

[Authorize]
public class VisitsController(ApplicationDbContext db, VisitService visits) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var query = db.Visits
            .Include(x => x.Person)
            .Include(x => x.VisitingUnit)
            .AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Person.FirstName.Contains(search) || x.Person.LastName.Contains(search) || x.Person.NationalCode.Contains(search));
        return View(await query.OrderByDescending(x => x.EntryDate).ThenByDescending(x => x.EntryTime).Take(500).ToListAsync());
    }

    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
        ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
        return View(new VisitEntryViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitEntryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
            ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
            return View(model);
        }

        var error = await visits.RegisterEntryAsync(model.PersonId, model.VisitingUnitId, model.EntryTime, model.EntryDescription);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            ViewBag.Persons = await db.Persons.Where(x => x.IsActive).OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync();
            ViewBag.Units = await db.OrganizationalUnits.Where(x => x.IsActive).OrderBy(x => x.Title).ToListAsync();
            return View(model);
        }

        TempData["Success"] = "ورود با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    public async Task<IActionResult> Exit(long id)
    {
        var visit = await db.Visits.Include(x => x.Person).FirstOrDefaultAsync(x => x.VisitId == id);
        if (visit is null) return NotFound();
        if (!visit.IsOpen)
        {
            TempData["Error"] = "خروج این مراجعه قبلاً ثبت شده است.";
            return RedirectToAction(nameof(Index));
        }
        ViewBag.PersonName = $"{visit.Person.FirstName} {visit.Person.LastName}";
        return View(new VisitExitViewModel { VisitId = id });
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Exit(VisitExitViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var error = await visits.RegisterExitAsync(model.VisitId, model.ExitTime, model.ExitDescription);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            return View(model);
        }
        TempData["Success"] = "خروج با موفقیت ثبت شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Open()
    {
        var visits = await db.Visits.Include(x => x.Person).Include(x => x.VisitingUnit)
            .Where(x => x.ExitDate == null && x.ExitTime == null)
            .OrderBy(x => x.EntryTime).AsNoTracking().ToListAsync();
        return View(visits);
    }
}
