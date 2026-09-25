using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

/// <summary>
/// Keeps older assignment databases compatible when new profile, activity and notification fields are introduced.
/// Fresh databases are still created by EF EnsureCreated; this updater only fills gaps on an existing database.
/// </summary>
public static class DatabaseSchemaUpdater
{
    public static async Task EnsureLatestAsync(ApplicationDbContext db)
    {
        if (!await ColumnExistsAsync(db, "Users", "Username"))
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Users ADD COLUMN Username varchar(60) NULL;");
        }

        if (!await ColumnExistsAsync(db, "Users", "Bio"))
        {
            await db.Database.ExecuteSqlRawAsync(
                "ALTER TABLE Users ADD COLUMN Bio varchar(500) NULL;");
        }

        var users = await db.Users
            .Select(u => new { u.Id, u.FullName, u.Email })
            .OrderBy(u => u.Id)
            .ToListAsync();

        var usernameRows = await db.Database
            .SqlQueryRaw<UserUsernameRow>(
                "SELECT Id, Username FROM Users ORDER BY Id")
            .ToListAsync();

        var usernames = usernameRows
            .ToDictionary(u => u.Id, u => u.Username ?? string.Empty);

        var usedUsernames = usernames.Values
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var user in users.Where(u => string.IsNullOrWhiteSpace(usernames.GetValueOrDefault(u.Id))))
        {
            var baseName = BuildUsername(user.FullName, user.Email, user.Id);
            var candidate = baseName;
            var suffix = 2;

            while (usedUsernames.Contains(candidate))
            {
                candidate = $"{baseName}{suffix++}";
            }

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Users SET Username = {0} WHERE Id = {1}",
                candidate,
                user.Id);
            usedUsernames.Add(candidate);
            usernames[user.Id] = candidate;
        }

        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Users MODIFY COLUMN Username varchar(60) NOT NULL;");

        var usernameIndexExists = await IndexExistsAsync(db, "Users", "IX_Users_Username");
        if (!usernameIndexExists)
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

    private static async Task<bool> ColumnExistsAsync(
        ApplicationDbContext db,
        string tableName,
        string columnName)
    {
        var count = await db.Database
            .SqlQueryRaw<long>(
                "SELECT COUNT(*) AS Value FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = {0} AND column_name = {1}",
                tableName,
                columnName)
            .SingleAsync();

        return count > 0;
    }

    private static async Task<bool> IndexExistsAsync(
        ApplicationDbContext db,
        string tableName,
        string indexName)
    {
        var count = await db.Database
            .SqlQueryRaw<long>(
                "SELECT COUNT(*) AS Value FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = {0} AND index_name = {1}",
                tableName,
                indexName)
            .SingleAsync();

        return count > 0;
    }

    private sealed class UserUsernameRow
    {
        public int Id { get; set; }
        public string? Username { get; set; }
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
