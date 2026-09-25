using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class AnnouncementsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var items = await db.Announcements.OrderByDescending(a => a.PublishedAt).ToListAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create() => View(new Announcement());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Announcement model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        db.Announcements.Add(model);
        await db.SaveChangesAsync();

        if (model.IsPublished)
        {
            var users = await db.Users.Select(u => u.Id).ToListAsync();
            db.Notifications.AddRange(users.Select(userId => new Notification
            {
                UserId = userId,
                Type = "Announcement",
                Title = model.Title,
                Message = model.Message,
                LinkUrl = "/Notifications",
                CreatedAt = DateTime.UtcNow
            }));
            await db.SaveChangesAsync();
        }

        TempData["Success"] = "Announcement published.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await db.Announcements.FindAsync(id);
        return item is null ? NotFound() : View(item);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Announcement model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var item = await db.Announcements.FindAsync(id);
        if (item is null)
        {
            return NotFound();
        }

        var wasPublished = item.IsPublished;
        item.Title = model.Title;
        item.Message = model.Message;
        item.PublishedAt = model.PublishedAt;
        item.IsPublished = model.IsPublished;
        await db.SaveChangesAsync();

        if (!wasPublished && model.IsPublished)
        {
            var users = await db.Users.Select(u => u.Id).ToListAsync();
            db.Notifications.AddRange(users.Select(userId => new Notification
            {
                UserId = userId,
                Type = "Announcement",
                Title = model.Title,
                Message = model.Message,
                LinkUrl = "/Notifications",
                CreatedAt = DateTime.UtcNow
            }));
            await db.SaveChangesAsync();
        }

        TempData["Success"] = "Announcement updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Announcements.FindAsync(id);
        if (item is not null)
        {
            db.Announcements.Remove(item);
            await db.SaveChangesAsync();
            TempData["Success"] = "Announcement deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
