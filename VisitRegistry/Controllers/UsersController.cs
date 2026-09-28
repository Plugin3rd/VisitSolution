using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Models;
using VisitRegistry.ViewModels;

namespace VisitRegistry.Controllers;

[Authorize(Roles = "مدیر سامانه")]
public class UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = userManager.Users.OrderBy(x => x.UserName).ToList();
        var rows = new List<(ApplicationUser User, string Role)>();
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            rows.Add((user, roles.FirstOrDefault() ?? "بدون نقش"));
        }
        return View(rows);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
        return View(new UserCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (!await roleManager.RoleExistsAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "نقش انتخاب‌شده معتبر نیست.");

        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.UserName,
            DisplayName = model.DisplayName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            ViewBag.Roles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.Role);
        TempData["Success"] = "کاربر با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }
}
