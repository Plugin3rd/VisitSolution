using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitRegistry.Models;
using VisitRegistry.Services;
using VisitRegistry.ViewModels;

namespace VisitRegistry.Controllers;

[Authorize(Roles = "مدیر سامانه")]
public class UsersController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ICurrentUserService currentUser, AuditService audit) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        search = search?.Trim();

        var users = await userManager.Users
            .OrderBy(x => x.UserName)
            .ToListAsync();

        var rows = new List<(ApplicationUser User, string Role)>();

        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "بدون نقش";

            if (!string.IsNullOrWhiteSpace(search))
            {
                var statusMatches =
                    search.Equals("فعال", StringComparison.OrdinalIgnoreCase) ? user.IsActive :
                    search.Equals("غیرفعال", StringComparison.OrdinalIgnoreCase) ? !user.IsActive :
                    false;

                var matches =
                    (user.UserName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (user.DisplayName?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    role.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    statusMatches;

                if (!matches)
                    continue;
            }

            rows.Add((user, role));
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
            await LoadRolesAsync();
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
            await LoadRolesAsync();
            return View(model);
        }

        await userManager.AddToRoleAsync(user, model.Role);
        await audit.LogAsync(nameof(ApplicationUser), user.Id, "ایجاد", null, new { user.UserName, user.DisplayName, model.Role });
        TempData["Success"] = "کاربر با موفقیت ایجاد شد.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();
        var role = (await userManager.GetRolesAsync(user)).FirstOrDefault() ?? string.Empty;
        await LoadRolesAsync();
        return View(new UserEditViewModel
        {
            UserName = user.UserName ?? string.Empty,
            DisplayName = user.DisplayName,
            IsActive = user.IsActive,
            Role = role
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, UserEditViewModel model)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        if (!await roleManager.RoleExistsAsync(model.Role))
            ModelState.AddModelError(nameof(model.Role), "نقش انتخاب‌شده معتبر نیست.");

        var existing = await userManager.FindByNameAsync(model.UserName);
        if (existing is not null && existing.Id != user.Id)
            ModelState.AddModelError(nameof(model.UserName), "این نام کاربری قبلاً استفاده شده است.");

        if (!ModelState.IsValid)
        {
            await LoadRolesAsync();
            return View(model);
        }

        var oldValues = new { user.UserName, user.DisplayName, user.IsActive, Role = (await userManager.GetRolesAsync(user)).FirstOrDefault() };
        user.UserName = model.UserName;
        user.DisplayName = model.DisplayName;
        user.IsActive = model.IsActive;
        user.NormalizedUserName = userManager.NormalizeName(model.UserName);

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
            await LoadRolesAsync();
            return View(model);
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        if (currentRoles.Any()) await userManager.RemoveFromRolesAsync(user, currentRoles);
        await userManager.AddToRoleAsync(user, model.Role);

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await userManager.ResetPasswordAsync(user, token, model.Password);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                await LoadRolesAsync();
                return View(model);
            }
        }

        await audit.LogAsync(nameof(ApplicationUser), user.Id, "ویرایش", oldValues, new { user.UserName, user.DisplayName, user.IsActive, Role = model.Role });
        TempData["Success"] = "اطلاعات کاربر با موفقیت ویرایش شد.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (id == currentUser.UserId)
        {
            TempData["Error"] = "امکان حذف کاربر جاری وجود ندارد.";
            return RedirectToAction(nameof(Index));
        }

        var user = await userManager.FindByIdAsync(id);
        if (user is null) return NotFound();

        var oldValues = new { user.UserName, user.DisplayName, user.IsActive };
        var result = await userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            TempData["Error"] = "حذف کاربر انجام نشد: " + string.Join("، ", result.Errors.Select(x => x.Description));
            return RedirectToAction(nameof(Index));
        }

        await audit.LogAsync(nameof(ApplicationUser), id, "حذف", oldValues, null);
        TempData["Success"] = "کاربر با موفقیت حذف شد.";
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadRolesAsync() => ViewBag.Roles = await roleManager.Roles.OrderBy(x => x.Name).ToListAsync();
}
