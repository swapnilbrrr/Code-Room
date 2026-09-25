using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class ResourcesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var resources = await db.Resources.OrderBy(r => r.Title).ToListAsync();
        var courseNames = await db.Courses.ToDictionaryAsync(c => c.Id, c => c.Title);
        ViewBag.CourseNames = courseNames;
        return View(resources);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = await CourseSelectAsync(null);
        return View(new Resource());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Resource model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await CourseSelectAsync(model.CourseId);
            return View(model);
        }
        db.Resources.Add(model);
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Created", "Resource", model.Title, $"Created resource {model.Title}.");
        TempData["Success"] = "Resource created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var resource = await db.Resources.FindAsync(id);
        if (resource is null)
        {
            return NotFound();
        }
        ViewBag.Courses = await CourseSelectAsync(resource.CourseId);
        return View(resource);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Resource model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await CourseSelectAsync(model.CourseId);
            return View(model);
        }

        var resource = await db.Resources.FindAsync(id);
        if (resource is null)
        {
            return NotFound();
        }
        resource.Title = model.Title;
        resource.Url = model.Url;
        resource.Type = model.Type;
        resource.CourseId = model.CourseId;
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Updated", "Resource", resource.Title, $"Updated resource {resource.Title}.");
        TempData["Success"] = "Resource updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var resource = await db.Resources.FindAsync(id);
        if (resource is not null)
        {
            var deletedTitle = resource.Title;
            db.Resources.Remove(resource);
            await db.SaveChangesAsync();
            await AdminAuditService.RecordAsync(db, User.GetUserId(), "Deleted", "Resource", deletedTitle, $"Deleted resource {deletedTitle}.");
            TempData["Success"] = "Resource deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> CourseSelectAsync(int? selected) =>
        new(await db.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
}
