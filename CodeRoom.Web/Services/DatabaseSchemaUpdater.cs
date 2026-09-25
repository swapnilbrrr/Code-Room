using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

/// <summary>
/// Keeps older assignment databases compatible when new profile/activity fields are introduced.
/// Fresh databases are still created by EF EnsureCreated; this updater only fills gaps on an existing database.
/// </summary>
public static class DatabaseSchemaUpdater
{
    public static async Task EnsureLatestAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Users ADD COLUMN IF NOT EXISTS Username varchar(60) NULL;");

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Users ADD COLUMN IF NOT EXISTS Bio varchar(500) NULL;");

        var users = await db.Users.OrderBy(u => u.Id).ToListAsync();
        var usedUsernames = users
            .Select(u => u.Username)
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var user in users.Where(u => string.IsNullOrWhiteSpace(u.Username)))
        {
            var baseName = BuildUsername(user.FullName, user.Email, user.Id);
            var candidate = baseName;
            var suffix = 2;

            while (usedUsernames.Contains(candidate))
            {
                candidate = $"{baseName}{suffix++}";
            }

            user.Username = candidate;
            usedUsernames.Add(candidate);
        }

        if (users.Any(u => !string.IsNullOrWhiteSpace(u.Username)))
        {
            await db.SaveChangesAsync();
        }

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Users MODIFY COLUMN Username varchar(60) NOT NULL;");

        var usernameIndexExists = await db.Database
            .SqlQueryRaw<long>("SELECT COUNT(*) AS Value FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = 'Users' AND index_name = 'IX_Users_Username'")
            .SingleAsync();

        if (usernameIndexExists == 0)
        {
            await db.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IX_Users_Username ON Users (Username);");
        }

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS UserActivities (
    Id int NOT NULL AUTO_INCREMENT,
    UserId int NOT NULL,
    ActivityType varchar(40) NOT NULL,
    Description varchar(220) NOT NULL,
    CreatedAt datetime(6) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_UserActivities_UserId (UserId),
    INDEX IX_UserActivities_UserId_CreatedAt (UserId, CreatedAt),
    CONSTRAINT FK_UserActivities_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS Notifications (
    Id int NOT NULL AUTO_INCREMENT,
    UserId int NOT NULL,
    Type varchar(40) NOT NULL,
    Title varchar(150) NOT NULL,
    Message varchar(500) NOT NULL,
    LinkUrl varchar(300) NULL,
    CreatedAt datetime(6) NOT NULL,
    IsRead tinyint(1) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_Notifications_UserId (UserId),
    INDEX IX_Notifications_UserId_IsRead_CreatedAt (UserId, IsRead, CreatedAt),
    CONSTRAINT FK_Notifications_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");
    }

    private static string BuildUsername(string fullName, string email, int id)
    {
        var source = fullName.Trim();
        if (string.IsNullOrWhiteSpace(source))
        {
            source = email.Split('@')[0];
        }

        var cleaned = new string(source
            .ToLowerInvariant()
            .Where(char.IsLetterOrDigit)
            .ToArray());

        return string.IsNullOrWhiteSpace(cleaned)
            ? $"learner{id}"
            : cleaned.Length > 50
                ? cleaned[..50]
                : cleaned;
    }
}
