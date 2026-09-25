using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class ChallengesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var challenges = await db.Challenges
            .Include(c => c.Course)
            .Include(c => c.Lesson)
            .OrderBy(c => c.Course.Title)
            .ThenBy(c => c.Title)
            .ToListAsync();

        return View(challenges);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await LoadCoursesAsync();
        await LoadLessonsAsync(null, null);
        return View(new ChallengeFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ChallengeFormViewModel model)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == model.CourseId))
        {
            ModelState.AddModelError(nameof(model.CourseId), "Select a valid course.");
        }

        if (model.LessonId is not null && !await db.Lessons.AnyAsync(l => l.Id == model.LessonId && l.CourseId == model.CourseId))
        {
            ModelState.AddModelError(nameof(model.LessonId), "Select a lesson from the chosen course.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(model.CourseId);
            await LoadLessonsAsync(model.CourseId, model.LessonId);
            return View(model);
        }

        var entity = ToEntity(model);
        db.Challenges.Add(entity);
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Created", "Challenge", entity.Title, $"Created practice challenge {entity.Title}.");

        TempData["Success"] = "Challenge created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var challenge = await db.Challenges.FindAsync(id);
        if (challenge is null) return NotFound();

        await LoadCoursesAsync(challenge.CourseId);
        await LoadLessonsAsync(challenge.CourseId, challenge.LessonId);
        return View(ToModel(challenge));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ChallengeFormViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!await db.Courses.AnyAsync(c => c.Id == model.CourseId))
        {
            ModelState.AddModelError(nameof(model.CourseId), "Select a valid course.");
        }

        if (model.LessonId is not null && !await db.Lessons.AnyAsync(l => l.Id == model.LessonId && l.CourseId == model.CourseId))
        {
            ModelState.AddModelError(nameof(model.LessonId), "Select a lesson from the chosen course.");
        }

        if (!ModelState.IsValid)
        {
            await LoadCoursesAsync(model.CourseId);
            await LoadLessonsAsync(model.CourseId, model.LessonId);
            model.IsEdit = true;
            return View(model);
        }

        var challenge = await db.Challenges.FindAsync(id);
        if (challenge is null) return NotFound();

        challenge.CourseId = model.CourseId;
        challenge.LessonId = model.LessonId;
        challenge.Title = model.Title.Trim();
        challenge.Instructions = model.Instructions.Trim();
        challenge.StarterCode = model.StarterCode?.Trim();
        challenge.Hint = model.Hint?.Trim();
        challenge.ExpectedAnswer = model.ExpectedAnswer.Trim();
        challenge.ValidationMode = model.ValidationMode;
        challenge.Points = model.Points;

        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Updated", "Challenge", challenge.Title, $"Updated practice challenge {challenge.Title}.");

        TempData["Success"] = "Challenge updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var challenge = await db.Challenges.FindAsync(id);
        if (challenge is not null)
        {
            var title = challenge.Title;
            db.Challenges.Remove(challenge);
            await db.SaveChangesAsync();
            await AdminAuditService.RecordAsync(db, User.GetUserId(), "Deleted", "Challenge", title, $"Deleted practice challenge {title}.");
            TempData["Success"] = "Challenge deleted.";
        }

        return RedirectToAction(nameof(Index));
    }

    private Challenge ToEntity(ChallengeFormViewModel m) => new()
    {
        CourseId = m.CourseId,
        LessonId = m.LessonId,
        Title = m.Title.Trim(),
        Instructions = m.Instructions.Trim(),
        StarterCode = m.StarterCode?.Trim(),
        Hint = m.Hint?.Trim(),
        ExpectedAnswer = m.ExpectedAnswer.Trim(),
        ValidationMode = m.ValidationMode,
        Points = m.Points
    };

    private static ChallengeFormViewModel ToModel(Challenge c) => new()
    {
        Id = c.Id,
        CourseId = c.CourseId,
        LessonId = c.LessonId,
        Title = c.Title,
        Instructions = c.Instructions,
        StarterCode = c.StarterCode,
        Hint = c.Hint,
        ExpectedAnswer = c.ExpectedAnswer,
        ValidationMode = c.ValidationMode,
        Points = c.Points,
        IsEdit = true
    };

    private async Task LoadCoursesAsync(int? selected = null)
    {
        ViewBag.Courses = new SelectList(await db.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
    }

    private async Task LoadLessonsAsync(int? courseId, int? selected)
    {
        List<Lesson> lessons = courseId is null
            ? []
            : await db.Lessons.Where(l => l.CourseId == courseId).OrderBy(l => l.Order).ToListAsync();

        ViewBag.Lessons = new SelectList(lessons, "Id", "Title", selected);
    }
}
