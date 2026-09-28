using Microsoft.EntityFrameworkCore;
using VisitRegistry.Data;
using VisitRegistry.Models;

namespace VisitRegistry.Services;

public class VisitService(ApplicationDbContext db, ICurrentUserService currentUser, AuditService audit)
{
    public async Task<(bool Allowed, string Message)> CanEnterAsync(int personId)
    {
        var person = await db.Persons.FindAsync(personId);
        if (person is null) return (false, "شخص مورد نظر یافت نشد.");
        if (!person.IsActive) return (false, "وضعیت این شخص غیرفعال است.");

        var today = DateTime.Today;
        var blackListed = await db.BlackLists.AnyAsync(x =>
            x.PersonId == personId && x.IsActive && x.StartDate.Date <= today &&
            (!x.EndDate.HasValue || x.EndDate.Value.Date >= today));
        if (blackListed)
            return (false, "امکان ثبت ورود وجود ندارد. این شخص در فهرست ممنوع‌الورود قرار دارد.");

        var openVisit = await db.Visits.AnyAsync(x => x.PersonId == personId && x.ExitDate == null && x.ExitTime == null);
        if (openVisit)
            return (false, "برای این شخص یک مراجعه باز وجود دارد. ابتدا خروج مراجعه قبلی را ثبت کنید.");

        return (true, "");
    }

    public async Task<string?> RegisterEntryAsync(int personId, int unitId, TimeSpan time, string? description)
    {
        var check = await CanEnterAsync(personId);
        if (!check.Allowed) return check.Message;

        var visit = new Visit
        {
            PersonId = personId,
            VisitingUnitId = unitId,
            EntryDate = DateTime.Today,
            EntryTime = time,
            EntryDescription = description,
            CreatedByUserId = currentUser.UserId,
            CreatedAt = DateTime.UtcNow
        };
        db.Visits.Add(visit);
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(Visit), visit.VisitId.ToString(), "ثبت ورود", null, new { visit.PersonId, visit.VisitingUnitId });
        return null;
    }

    public async Task<string?> RegisterExitAsync(long visitId, TimeSpan time, string? description)
    {
        var visit = await db.Visits.FindAsync(visitId);
        if (visit is null) return "مراجعه مورد نظر یافت نشد.";
        if (!visit.IsOpen) return "خروج این مراجعه قبلاً ثبت شده است.";

        visit.ExitDate = DateTime.Today;
        visit.ExitTime = time;
        visit.ExitDescription = description;
        visit.LastModifiedByUserId = currentUser.UserId;
        visit.LastModifiedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        await audit.LogAsync(nameof(Visit), visit.VisitId.ToString(), "ثبت خروج", null, new { visit.ExitDate, visit.ExitTime });
        return null;
    }
}
