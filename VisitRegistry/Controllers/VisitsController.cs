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
        ViewBag.Units = await db.OrganizationalUnits
            .Where(x => x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();

        return View(new VisitEntryViewModel());
    }

    [HttpGet]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    public async Task<IActionResult> SearchPersons(string? term)
    {
        term = term?.Trim();
        if (string.IsNullOrWhiteSpace(term))
            return Json(Array.Empty<object>());

        term = NormalizeDigits(term);

        var query = db.Persons
            .Where(x => x.IsActive)
            .AsNoTracking();

        if (term.Length >= 2)
        {
            query = query.Where(x =>
                x.FirstName.Contains(term) ||
                x.LastName.Contains(term) ||
                x.NationalCode.Contains(term) ||
                x.Mobile.Contains(term));
        }

        var persons = await query
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .Take(30)
            .Select(x => new
            {
                id = x.PersonId,
                name = x.FirstName + " " + x.LastName,
                nationalCode = x.NationalCode,
                mobile = x.Mobile,
                unit = x.OrganizationalUnit != null ? x.OrganizationalUnit.Title : ""
            })
            .ToListAsync();

        return Json(persons);
    }

    [HttpPost]
    [Authorize(Roles = "مدیر سامانه,اپراتور")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VisitEntryViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Units = await db.OrganizationalUnits
                .Where(x => x.IsActive)
                .OrderBy(x => x.Title)
                .ToListAsync();
            return View(model);
        }

        var error = await visits.RegisterEntryAsync(model.PersonId, model.VisitingUnitId, model.EntryTime, model.EntryDescription);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            ViewBag.Units = await db.OrganizationalUnits
                .Where(x => x.IsActive)
                .OrderBy(x => x.Title)
                .ToListAsync();
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

    public async Task<IActionResult> Open(string? search)
    {
        search = search?.Trim();

        var query = db.Visits
            .Include(x => x.Person)
            .Include(x => x.VisitingUnit)
            .Where(x => x.ExitDate == null && x.ExitTime == null)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = NormalizeDigits(search);

            query = query.Where(x =>
                x.Person.FirstName.Contains(search) ||
                x.Person.LastName.Contains(search) ||
                x.Person.NationalCode.Contains(normalizedSearch) ||
                x.Person.Mobile.Contains(normalizedSearch) ||
                x.VisitingUnit.Title.Contains(search) ||
                (x.EntryDescription != null && x.EntryDescription.Contains(search)));
        }

        var visits = await query
            .OrderBy(x => x.EntryTime)
            .ToListAsync();

        return View(visits);
    }

    private static string NormalizeDigits(string value)
    {
        const string persian = "۰۱۲۳۴۵۶۷۸۹";
        const string arabic = "٠١٢٣٤٥٦٧٨٩";
        const string latin = "0123456789";

        var chars = value.ToCharArray();
        for (var i = 0; i < chars.Length; i++)
        {
            var p = persian.IndexOf(chars[i]);
            if (p >= 0)
            {
                chars[i] = latin[p];
                continue;
            }

            var a = arabic.IndexOf(chars[i]);
            if (a >= 0)
                chars[i] = latin[a];
        }

        return new string(chars);
    }
}
