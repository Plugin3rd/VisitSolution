using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Models;

namespace VisitRegistry.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = ["مدیر سامانه", "اپراتور", "مشاهده‌گر"];

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var username = configuration["SeedAdmin:UserName"] ?? "admin";
        var password = configuration["SeedAdmin:Password"] ?? "ChangeMe_12345";
        var displayName = configuration["SeedAdmin:DisplayName"] ?? "مدیر سامانه";

        var admin = await userManager.FindByNameAsync(username);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = username,
                DisplayName = displayName,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            var result = await userManager.CreateAsync(admin, password);
            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join(" | ", result.Errors.Select(e => e.Description)));
        }

        if (!await userManager.IsInRoleAsync(admin, "مدیر سامانه"))
            await userManager.AddToRoleAsync(admin, "مدیر سامانه");

        if (!await db.OrganizationalUnits.AnyAsync())
        {
            db.OrganizationalUnits.AddRange(
                new OrganizationalUnit { Title = "مدیریت", UnitType = UnitType.ActivityUnit, IsActive = true },
                new OrganizationalUnit { Title = "حراست", UnitType = UnitType.ActivityUnit, IsActive = true },
                new OrganizationalUnit { Title = "منابع انسانی", UnitType = UnitType.ActivityUnit, IsActive = true },
                new OrganizationalUnit { Title = "امور مالی", UnitType = UnitType.ActivityUnit, IsActive = true },
                new OrganizationalUnit { Title = "فناوری اطلاعات", UnitType = UnitType.ActivityUnit, IsActive = true },
                new OrganizationalUnit { Title = "دپارتمان обслугуه", UnitType = UnitType.VisitingUnit, IsActive = true },
                new OrganizationalUnit { Title = "دفتر امور اداری", UnitType = UnitType.VisitingUnit, IsActive = true },
                new OrganizationalUnit { Title = "بخش پزشک قانونی", UnitType = UnitType.VisitingUnit, IsActive = true });
            await db.SaveChangesAsync();
        }
    }
}
