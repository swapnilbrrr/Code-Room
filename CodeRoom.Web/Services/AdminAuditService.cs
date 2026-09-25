using CodeRoom.Web.Data;
using CodeRoom.Web.Models;

namespace CodeRoom.Web.Services;

public static class AdminAuditService
{
    public static async Task RecordAsync(
        ApplicationDbContext db,
        int userId,
        string action,
        string entityType,
        string? entityName,
        string description)
    {
        db.AdminAuditLogs.Add(new AdminAuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityName = entityName,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}
