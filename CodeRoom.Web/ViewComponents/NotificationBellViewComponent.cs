using CodeRoom.Web.Data;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.ViewComponents;

public class NotificationBellViewComponent(ApplicationDbContext db) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return Content(string.Empty);
        }

        var userId = User.GetUserId();

        var notifications = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(6)
            .ToListAsync();

        var announcements = await db.Announcements
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt)
            .Take(5)
            .ToListAsync();

        announcements = announcements
            .Where(a => !notifications.Any(n =>
                n.Type == "Announcement" &&
                n.Title == a.Title &&
                n.Message == a.Message))
            .Take(3)
            .ToList();

        var items = notifications
            .Select(n => new NotificationItemViewModel
            {
                NotificationId = n.Id,
                Type = n.Type,
                Title = n.Title,
                Message = n.Message,
                LinkUrl = n.LinkUrl,
                CreatedAt = n.CreatedAt,
                IsRead = n.IsRead,
                IsAnnouncement = false
            })
            .Concat(announcements.Select(a => new NotificationItemViewModel
            {
                Type = "Announcement",
                Title = a.Title,
                Message = a.Message,
                LinkUrl = "/Notifications",
                CreatedAt = a.PublishedAt,
                IsRead = true,
                IsAnnouncement = true
            }))
            .OrderByDescending(x => x.CreatedAt)
            .Take(7)
            .ToList();

        var unread = await db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

        return View(new NotificationBellViewModel
        {
            UnreadCount = unread,
            Items = items
        });
    }
}
