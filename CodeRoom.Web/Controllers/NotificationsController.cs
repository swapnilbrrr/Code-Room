using CodeRoom.Web.Data;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class NotificationsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = User.GetUserId();
        var notifications = await db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        var announcements = await db.Announcements
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync();

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
            .ToList();

        return View(new NotificationsPageViewModel
        {
            UnreadCount = notifications.Count(n => !n.IsRead),
            Notifications = items
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = User.GetUserId();
        var unread = await db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in unread)
        {
            notification.IsRead = true;
        }

        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id)
    {
        var userId = User.GetUserId();
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

        if (notification is not null)
        {
            notification.IsRead = true;
            await db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
