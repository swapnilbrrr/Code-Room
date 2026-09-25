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

        db.Lessons.Add(model);
        await db.SaveChangesAsync();
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
        lesson.Content = model.Content;
        lesson.VideoUrl = model.VideoUrl;
        lesson.ResourceUrl = model.ResourceUrl;
        lesson.Order = model.Order;
        lesson.IsPublished = model.IsPublished;

        await db.SaveChangesAsync();
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
            db.Lessons.Remove(lesson);
            await db.SaveChangesAsync();
            TempData["Success"] = "Lesson deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> CourseSelectAsync(int? selected) =>
        new(await db.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
}
