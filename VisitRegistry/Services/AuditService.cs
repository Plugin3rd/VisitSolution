using System.Text.Json;
using VisitRegistry.Data;
using VisitRegistry.Models;

namespace VisitRegistry.Services;

public class AuditService(ApplicationDbContext db, ICurrentUserService currentUser, IHttpContextAccessor http)
{
    public async Task LogAsync(string entityName, string entityId, string action, object? oldValues = null, object? newValues = null)
    {
        db.AuditLogs.Add(new AuditLog
        {
            UserId = currentUser.UserId,
            EntityName = entityName,
            EntityId = entityId,
            Action = action,
            OldValues = oldValues is null ? null : JsonSerializer.Serialize(oldValues),
            NewValues = newValues is null ? null : JsonSerializer.Serialize(newValues),
            IpAddress = http.HttpContext?.Connection.RemoteIpAddress?.ToString(),
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
