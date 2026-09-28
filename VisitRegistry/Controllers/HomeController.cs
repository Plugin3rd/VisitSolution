using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;

namespace VisitRegistry.Controllers;

[Authorize]
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.PersonCount = await db.Persons.CountAsync(x => x.IsActive);
        ViewBag.BlackListCount = await db.BlackLists.CountAsync(x => x.IsActive && x.StartDate <= DateTime.Today && (!x.EndDate.HasValue || x.EndDate >= DateTime.Today));
        ViewBag.TodayVisits = await db.Visits.CountAsync(x => x.EntryDate.Date == DateTime.Today);
        ViewBag.OpenVisits = await db.Visits.CountAsync(x => x.ExitDate == null && x.ExitTime == null);
        return View();
    }
}
