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
public class LessonsController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? courseId)
    {
        var query = db.Lessons.Include(l => l.Course).AsQueryable();
        if (courseId is not null)
        {
            query = query.Where(l => l.CourseId == courseId);
        }

        ViewBag.Courses = await CourseSelectAsync(courseId);
        ViewBag.CourseId = courseId;

        var lessons = await query
            .OrderBy(l => l.Course.Title).ThenBy(l => l.Order)
            .ToListAsync();
        return View(lessons);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? courseId)
    {
        ViewBag.Courses = await CourseSelectAsync(courseId);
        return View(new Lesson { CourseId = courseId ?? 0, Order = 1 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Lesson model)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == model.CourseId))
        {
            ModelState.AddModelError(nameof(model.CourseId), "Select a valid course.");
        }

        if (!ModelState.IsValid)
        {
            ViewBag.Courses = await CourseSelectAsync(model.CourseId);
            return View(model);
        }

        await AssignModuleAsync(model);
        db.Lessons.Add(model);
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Created", "Lesson", model.Title, $"Created lesson {model.Title}.");
        TempData["Success"] = "Lesson created.";
        return RedirectToAction(nameof(Index), new { courseId = model.CourseId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is null)
        {
            return NotFound();
        }
        ViewBag.Courses = await CourseSelectAsync(lesson.CourseId);
        return View(lesson);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Lesson model)
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

        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is null)
        {
            return NotFound();
        }

        lesson.CourseId = model.CourseId;
        lesson.Title = model.Title;
        lesson.Summary = model.Summary;
        lesson.Content = model.Content;
        lesson.ContentType = model.ContentType;
        lesson.VideoUrl = model.VideoUrl;
        lesson.AudioUrl = model.AudioUrl;
        lesson.ResourceUrl = model.ResourceUrl;
        lesson.DurationMinutes = model.DurationMinutes;
        lesson.Order = model.Order;
        lesson.IsPublished = model.IsPublished;
        lesson.CourseModuleId = null;
        await AssignModuleAsync(lesson);

        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Updated", "Lesson", lesson.Title, $"Updated lesson {lesson.Title}.");
        TempData["Success"] = "Lesson updated.";
        return RedirectToAction(nameof(Index), new { courseId = lesson.CourseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var lesson = await db.Lessons.FindAsync(id);
        if (lesson is not null)
        {
            var deletedTitle = lesson.Title;
            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();
            await AdminAuditService.RecordAsync(db, User.GetUserId(), "Deleted", "Lesson", deletedTitle, $"Deleted lesson {deletedTitle}.");
            TempData["Success"] = "Lesson deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task AssignModuleAsync(Lesson lesson)
    {
        var module = await db.CourseModules
            .Where(m => m.CourseId == lesson.CourseId)
            .OrderBy(m => m.Order)
            .ThenBy(m => m.Id)
            .ToListAsync();

        if (module.Count == 0)
        {
            var newModule = new CourseModule
            {
                CourseId = lesson.CourseId,
                Title = "Module 1 — Foundations",
                Description = "Foundational concepts and guided practice.",
                Order = 1
            };
            db.CourseModules.Add(newModule);
            await db.SaveChangesAsync();
            lesson.CourseModule = newModule;
            return;
        }

        var target = module
            .OrderBy(m => m.Lessons.Count)
            .ThenBy(m => m.Order)
            .First();

        lesson.CourseModule = target;
    }

    private async Task<SelectList> CourseSelectAsync(int? selected) =>
        new(await db.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
}
