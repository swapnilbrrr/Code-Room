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
        string notificationType = "Activity")
    {
        db.UserActivities.Add(new UserActivity
        {
            UserId = userId,
            ActivityType = activityType,
            Description = description,
            CreatedAt = DateTime.UtcNow
        });

        if (!string.IsNullOrWhiteSpace(notificationTitle) && !string.IsNullOrWhiteSpace(notificationMessage))
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
    }

    public static int CalculateStreak(IEnumerable<UserActivity> activities, DateTime? today = null)
    {
        var activeDays = activities
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
    }
}
