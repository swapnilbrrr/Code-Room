using CodeRoom.Web.Data;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

public static class DatabaseSchemaUpdater
{
    public static async Task EnsureLatestAsync(ApplicationDbContext db)
    {
        await EnsureColumnAsync(db, "Users", "Username", "varchar(60) NULL");
        await EnsureColumnAsync(db, "Users", "Bio", "varchar(500) NULL");
        await EnsureColumnAsync(db, "Users", "AvatarUrl", "varchar(300) NULL");
        await EnsureColumnAsync(db, "Users", "Xp", "int NOT NULL DEFAULT 0");
        await EnsureColumnAsync(db, "Users", "ThemePreference", "varchar(20) NOT NULL DEFAULT 'system'");
        await EnsureColumnAsync(db, "Users", "ProfileVisibility", "varchar(20) NOT NULL DEFAULT 'Public'");
        await EnsureColumnAsync(db, "Users", "EmailNotificationsEnabled", "tinyint(1) NOT NULL DEFAULT 1");

        await EnsureColumnAsync(db, "Courses", "EstimatedMinutes", "int NOT NULL DEFAULT 120");
        await EnsureColumnAsync(db, "Courses", "IsCertification", "tinyint(1) NOT NULL DEFAULT 0");
        await EnsureColumnAsync(db, "Courses", "CertificateName", "varchar(160) NULL");
        await EnsureColumnAsync(db, "Courses", "PassingScorePercent", "int NOT NULL DEFAULT 70");

        await EnsureColumnAsync(db, "Lessons", "CourseModuleId", "int NULL");
        await EnsureColumnAsync(db, "Lessons", "Summary", "varchar(180) NULL");
        await EnsureColumnAsync(db, "Lessons", "ContentType", "varchar(30) NOT NULL DEFAULT 'Reading'");
        await EnsureColumnAsync(db, "Lessons", "AudioUrl", "varchar(300) NULL");
        await EnsureColumnAsync(db, "Lessons", "DurationMinutes", "int NOT NULL DEFAULT 10");

        await EnsureColumnAsync(db, "Quizzes", "AssessmentType", "varchar(30) NOT NULL DEFAULT 'Quiz'");
        await EnsureColumnAsync(db, "Quizzes", "TimeLimitMinutes", "int NOT NULL DEFAULT 0");
        await EnsureColumnAsync(db, "Quizzes", "PassingScorePercent", "int NOT NULL DEFAULT 70");
        await EnsureColumnAsync(db, "Quizzes", "IsCertificationExam", "tinyint(1) NOT NULL DEFAULT 0");

        await EnsureUsernamesAsync(db);
        await EnsureUserConstraintsAsync(db);
        await EnsureCoreTablesAsync(db);
    }

    private static async Task EnsureUsernamesAsync(ApplicationDbContext db)
    {
        var rows = await db.Database
            .SqlQueryRaw<UserUsernameRow>(
                "SELECT Id, Username FROM Users ORDER BY Id")
            .ToListAsync();

        var users = await db.Users
            .IgnoreQueryFilters()
            .Select(u => new { u.Id, u.FullName, u.Email })
            .OrderBy(u => u.Id)
            .ToListAsync();

        var usernames = rows.ToDictionary(u => u.Id, u => u.Username ?? string.Empty);
        var used = usernames.Values
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .ToHashSet();

        foreach (var user in users.Where(u => string.IsNullOrWhiteSpace(usernames.GetValueOrDefault(u.Id))))
        {
            var baseName = BuildUsername(user.FullName, user.Email, user.Id);
            var candidate = baseName;
            var suffix = 2;

            while (used.Contains(candidate))
            {
                candidate = $"{baseName}{suffix++}";
            }

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE Users SET Username = {0} WHERE Id = {1}",
                candidate, user.Id);

            used.Add(candidate);
        }
    }

    private static async Task EnsureUserConstraintsAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync(
            "ALTER TABLE Users MODIFY COLUMN Username varchar(60) NOT NULL;");

        if (!await IndexExistsAsync(db, "Users", "IX_Users_Username"))
        {
            await db.Database.ExecuteSqlRawAsync(
                "CREATE UNIQUE INDEX IX_Users_Username ON Users (Username);");
        }
    }

    private static async Task EnsureCoreTablesAsync(ApplicationDbContext db)
    {
        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS CourseModules (
    Id int NOT NULL AUTO_INCREMENT,
    CourseId int NOT NULL,
    Title varchar(120) NOT NULL,
    Description varchar(500) NOT NULL,
    ModuleOrder int NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_CourseModules_CourseId (CourseId),
    CONSTRAINT FK_CourseModules_Courses_CourseId
        FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await EnsureForeignKeyAsync(db, "Lessons", "FK_Lessons_CourseModules_CourseModuleId",
            "ALTER TABLE Lessons ADD CONSTRAINT FK_Lessons_CourseModules_CourseModuleId FOREIGN KEY (CourseModuleId) REFERENCES CourseModules (Id) ON DELETE SET NULL;");

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

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS Challenges (
    Id int NOT NULL AUTO_INCREMENT,
    CourseId int NOT NULL,
    LessonId int NULL,
    Title varchar(160) NOT NULL,
    Instructions varchar(1800) NOT NULL,
    StarterCode varchar(2000) NULL,
    Hint varchar(600) NULL,
    ExpectedAnswer varchar(1000) NOT NULL,
    ValidationMode varchar(30) NOT NULL,
    Points int NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_Challenges_CourseId (CourseId),
    INDEX IX_Challenges_LessonId (LessonId),
    CONSTRAINT FK_Challenges_Courses_CourseId
        FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Challenges_Lessons_LessonId
        FOREIGN KEY (LessonId) REFERENCES Lessons (Id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS Achievements (
    Id int NOT NULL AUTO_INCREMENT,
    Name varchar(100) NOT NULL,
    Description varchar(500) NOT NULL,
    Code varchar(40) NOT NULL,
    Icon varchar(20) NOT NULL,
    XpReward int NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE INDEX IX_Achievements_Code (Code)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS UserAchievements (
    Id int NOT NULL AUTO_INCREMENT,
    UserId int NOT NULL,
    AchievementId int NOT NULL,
    EarnedAt datetime(6) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE INDEX IX_UserAchievements_UserId_AchievementId (UserId, AchievementId),
    INDEX IX_UserAchievements_AchievementId (AchievementId),
    CONSTRAINT FK_UserAchievements_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserAchievements_Achievements_AchievementId
        FOREIGN KEY (AchievementId) REFERENCES Achievements (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS Certificates (
    Id int NOT NULL AUTO_INCREMENT,
    UserId int NOT NULL,
    CourseId int NOT NULL,
    QuizAttemptId int NOT NULL,
    CertificateNumber varchar(40) NOT NULL,
    Title varchar(160) NOT NULL,
    IssuedAt datetime(6) NOT NULL,
    PRIMARY KEY (Id),
    UNIQUE INDEX IX_Certificates_CertificateNumber (CertificateNumber),
    INDEX IX_Certificates_UserId (UserId),
    INDEX IX_Certificates_CourseId (CourseId),
    CONSTRAINT FK_Certificates_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Certificates_Courses_CourseId
        FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE,
    CONSTRAINT FK_Certificates_QuizAttempts_QuizAttemptId
        FOREIGN KEY (QuizAttemptId) REFERENCES QuizAttempts (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");

        await db.Database.ExecuteSqlRawAsync("""
CREATE TABLE IF NOT EXISTS AdminAuditLogs (
    Id int NOT NULL AUTO_INCREMENT,
    UserId int NOT NULL,
    Action varchar(60) NOT NULL,
    EntityType varchar(80) NOT NULL,
    EntityName varchar(120) NULL,
    Description varchar(500) NOT NULL,
    CreatedAt datetime(6) NOT NULL,
    PRIMARY KEY (Id),
    INDEX IX_AdminAuditLogs_UserId_CreatedAt (UserId, CreatedAt),
    CONSTRAINT FK_AdminAuditLogs_Users_UserId
        FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
""");
    }

    private static async Task EnsureColumnAsync(ApplicationDbContext db, string tableName, string columnName, string definition)
    {
        if (!await ColumnExistsAsync(db, tableName, columnName))
        {
            await db.Database.ExecuteSqlRawAsync(
                $"ALTER TABLE {tableName} ADD COLUMN {columnName} {definition};");
        }
    }

    private static async Task EnsureForeignKeyAsync(ApplicationDbContext db, string tableName, string constraintName, string statement)
    {
        var exists = await db.Database
            .SqlQueryRaw<long>(
                "SELECT COUNT(*) AS Value FROM information_schema.table_constraints WHERE table_schema = DATABASE() AND table_name = {0} AND constraint_name = {1}",
                tableName, constraintName)
            .SingleAsync();

        if (exists == 0)
        {
            await db.Database.ExecuteSqlRawAsync(statement);
        }
    }

    private static async Task<bool> ColumnExistsAsync(ApplicationDbContext db, string tableName, string columnName)
    {
        var count = await db.Database
            .SqlQueryRaw<long>(
                "SELECT COUNT(*) AS Value FROM information_schema.columns WHERE table_schema = DATABASE() AND table_name = {0} AND column_name = {1}",
                tableName, columnName)
            .SingleAsync();

        return count > 0;
    }

    private static async Task<bool> IndexExistsAsync(ApplicationDbContext db, string tableName, string indexName)
    {
        var count = await db.Database
            .SqlQueryRaw<long>(
                "SELECT COUNT(*) AS Value FROM information_schema.statistics WHERE table_schema = DATABASE() AND table_name = {0} AND index_name = {1}",
                tableName, indexName)
            .SingleAsync();

        return count > 0;
    }

    private static string BuildUsername(string fullName, string email, int id)
    {
        var source = string.IsNullOrWhiteSpace(fullName) ? email.Split('@')[0] : fullName.Trim();
        var cleaned = new string(source.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());

        return string.IsNullOrWhiteSpace(cleaned)
            ? $"learner{id}"
            : cleaned.Length > 50 ? cleaned[..50] : cleaned;
    }

    private sealed class UserUsernameRow
    {
        public int Id { get; set; }
        public string? Username { get; set; }
    }
}
