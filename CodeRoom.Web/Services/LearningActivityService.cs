using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Services;

public static class LearningActivityService
{
    public static async Task RecordAsync(
        ApplicationDbContext db,
        int userId,
        string activityType,
        string description,
        string? notificationTitle = null,
        string? notificationMessage = null,
        string? linkUrl = null,
        string notificationType = "Activity",
        int? xpOverride = null)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId);
        var xp = xpOverride ?? GetXp(activityType);

        user.Xp += Math.Max(0, xp);

        db.UserActivities.Add(new UserActivity
        {
            UserId = userId,
            ActivityType = activityType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        if (!string.IsNullOrWhiteSpace(notificationTitle) && !string.IsNullOrWhiteSpace(notificationMessage)
            && user.EmailNotificationsEnabled)
        {
            db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = notificationType,
                Title = notificationTitle,
                Message = notificationMessage,
                LinkUrl = linkUrl,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
        await AwardEligibleAchievementsAsync(db, userId, activityType);
    }

    public static int GetXp(string activityType) => activityType switch
    {
        "AccountCreated" => 25,
        "ProfileUpdated" => 5,
        "CourseEnrolled" => 20,
        "LessonCompleted" => 20,
        "QuizAttempted" => 30,
        "ChallengeCompleted" => 50,
        "CourseCompleted" => 100,
        "ExamPassed" => 150,
        "CertificateEarned" => 200,
        _ => 10
    };

    public static int GetLevel(int xp) => Math.Max(1, (xp / 250) + 1);

    public static int GetLevelProgress(int xp) => xp % 250;

    public static int CalculateStreak(IEnumerable<UserActivity> activities, DateTime? today = null)
    {
        var qualifyingTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CourseEnrolled",
            "LessonCompleted",
            "QuizAttempted",
            "ChallengeCompleted",
            "CourseCompleted",
            "ExamPassed"
        };

        var activeDays = activities
            .Where(a => qualifyingTypes.Contains(a.ActivityType))
            .Select(a => a.CreatedAt.Date)
            .Distinct()
            .ToHashSet();

        var cursor = (today ?? DateTime.UtcNow).Date;

        if (!activeDays.Contains(cursor))
        {
            cursor = cursor.AddDays(-1);
        }

        var streak = 0;
        while (activeDays.Contains(cursor))
        {
            streak++;
            cursor = cursor.AddDays(-1);
        }

        return streak;
    }

    public static async Task TryRecordStreakMilestoneAsync(ApplicationDbContext db, int userId)
    {
        var activities = await db.UserActivities
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(400)
            .ToListAsync();

        var streak = CalculateStreak(activities);
        var milestones = new[] { 3, 7, 14, 30 };

        foreach (var milestone in milestones)
        {
            if (streak < milestone)
            {
                continue;
            }

            var title = $"{milestone}-day learning streak";
            var exists = await db.Notifications.AnyAsync(n =>
                n.UserId == userId &&
                n.Type == "StreakMilestone" &&
                n.Title == title);

            if (exists)
            {
                continue;
            }

            db.Notifications.Add(new Notification
            {
                UserId = userId,
                Type = "StreakMilestone",
                Title = title,
                Message = $"You have learned on {milestone} consecutive days. Keep the momentum going!",
                LinkUrl = "/Dashboard",
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();

        if (streak >= 7)
        {
            await TryAwardAchievementAsync(db, userId, "streak-7");
        }
    }

    private static async Task AwardEligibleAchievementsAsync(ApplicationDbContext db, int userId, string activityType)
    {
        switch (activityType)
        {
            case "LessonCompleted":
                if (!await db.UserAchievements.AnyAsync(x => x.UserId == userId && x.Achievement.Code == "first-lesson"))
                    await TryAwardAchievementAsync(db, userId, "first-lesson");
                break;
            case "QuizAttempted":
                await TryAwardAchievementAsync(db, userId, "quiz-starter");
                break;
            case "ChallengeCompleted":
                await TryAwardAchievementAsync(db, userId, "challenge-starter");
                break;
            case "CourseCompleted":
                await TryAwardAchievementAsync(db, userId, "course-finisher");
                break;
            case "CertificateEarned":
                await TryAwardAchievementAsync(db, userId, "certificate");
                break;
        }

        var user = await db.Users.FindAsync(userId);
        if (user is not null && user.Xp >= 500)
        {
            await TryAwardAchievementAsync(db, userId, "xp-500");
        }
    }

    public static async Task TryAwardAchievementAsync(ApplicationDbContext db, int userId, string code)
    {
        var achievement = await db.Achievements.FirstOrDefaultAsync(a => a.Code == code);
        if (achievement is null || await db.UserAchievements.AnyAsync(x => x.UserId == userId && x.AchievementId == achievement.Id))
        {
            return;
        }

        db.UserAchievements.Add(new UserAchievement
        {
            UserId = userId,
            AchievementId = achievement.Id,
            EarnedAt = DateTime.UtcNow
        });

        if (achievement.XpReward > 0)
        {
            var user = await db.Users.FindAsync(userId);
            if (user is not null)
            {
                user.Xp += achievement.XpReward;
            }
        }

        db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = "Achievement",
            Title = $"Achievement unlocked: {achievement.Name}",
            Message = achievement.Description,
            LinkUrl = "/Profile",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }
}
