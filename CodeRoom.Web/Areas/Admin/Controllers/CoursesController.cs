using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class CoursesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var courses = await db.Courses
            .Include(c => c.Lessons)
            .OrderBy(c => c.Title)
            .ToListAsync();
        return View(courses);
    }

    [HttpGet]
    public IActionResult Create() => View(new Course());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Course model)
    {
        if (await db.Courses.AnyAsync(c => c.Slug == model.Slug))
        {
            ModelState.AddModelError(nameof(model.Slug), "This slug is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        model.CreatedAt = DateTime.UtcNow;
        db.Courses.Add(model);
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Created", "Course", model.Title, $"Created course {model.Title}.");
        TempData["Success"] = "Course created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var course = await db.Courses.FindAsync(id);
        return course is null ? NotFound() : View(course);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Course model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (await db.Courses.AnyAsync(c => c.Slug == model.Slug && c.Id != id))
        {
            ModelState.AddModelError(nameof(model.Slug), "This slug is already in use.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var course = await db.Courses.FindAsync(id);
        if (course is null)
        {
            return NotFound();
        }

        course.Title = model.Title;
        course.Slug = model.Slug;
        course.Description = model.Description;
        course.Category = model.Category;
        course.Level = model.Level;
        course.EstimatedMinutes = model.EstimatedMinutes;
        course.IsCertification = model.IsCertification;
        course.CertificateName = model.CertificateName;
        course.PassingScorePercent = model.PassingScorePercent;
        course.ThumbnailUrl = model.ThumbnailUrl;
        course.IsPublished = model.IsPublished;

        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Updated", "Course", course.Title, $"Updated course {course.Title}.");
        TempData["Success"] = "Course updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await db.Courses.FindAsync(id);
        if (course is not null)
        {
            var deletedTitle = course.Title;
            db.Courses.Remove(course);
            await db.SaveChangesAsync();
            await AdminAuditService.RecordAsync(db, User.GetUserId(), "Deleted", "Course", deletedTitle, $"Deleted course {deletedTitle}.");
            TempData["Success"] = "Course deleted.";
        }
        return RedirectToAction(nameof(Index));
    }
}
