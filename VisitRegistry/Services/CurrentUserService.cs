using System.Security.Claims;

namespace VisitRegistry.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? UserId => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string UserDisplayName =>
        httpContextAccessor.HttpContext?.User.FindFirstValue("DisplayName")
        ?? httpContextAccessor.HttpContext?.User.Identity?.Name
        ?? "کاربر سامانه";
}
