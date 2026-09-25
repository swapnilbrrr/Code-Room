using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.ViewModels.Learning;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class ChallengesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int id)
    {
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == id && c.IsPublished);
        if (course is null) return NotFound();

        var challenges = await db.Challenges
            .Where(c => c.CourseId == id)
            .Include(c => c.Lesson)
            .OrderBy(c => c.Id)
            .ToListAsync();

        return View(new ChallengeListViewModel { Course = course, Challenges = challenges });
    }

    [HttpGet]
    public async Task<IActionResult> Take(int id)
    {
        var challenge = await db.Challenges
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id && c.Course.IsPublished);

        return challenge is null ? NotFound() : View(challenge);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id, string? answer)
    {
        var challenge = await db.Challenges
            .Include(c => c.Course)
            .FirstOrDefaultAsync(c => c.Id == id && c.Course.IsPublished);

        if (challenge is null) return NotFound();

        var normalizedAnswer = Normalize(answer);
        var expected = Normalize(challenge.ExpectedAnswer);
        var correct = challenge.ValidationMode.Equals("Contains", StringComparison.OrdinalIgnoreCase)
            ? normalizedAnswer.Contains(expected, StringComparison.OrdinalIgnoreCase)
            : normalizedAnswer.Equals(expected, StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            var userId = User.GetUserId();
            var activityExists = await db.UserActivities.AnyAsync(a =>
                a.UserId == userId &&
                a.ActivityType == "ChallengeCompleted" &&
                a.Description == $"Completed {challenge.Title}");

            if (!activityExists)
            {
                await LearningActivityService.RecordAsync(
                    db,
                    userId,
                    "ChallengeCompleted",
                    $"Completed {challenge.Title}",
                    "Challenge completed",
                    $"Nice work. You earned {challenge.Points} XP from {challenge.Title}.",
                    $"/Challenges/Take/{challenge.Id}",
                    "Challenge");
            }

            TempData["ToastTitle"] = "Challenge passed";
            TempData["ToastMessage"] = $"+{challenge.Points} XP · {challenge.Title}";
            TempData["ToastIcon"] = "⚡";
        }
        else
        {
            TempData["ToastTitle"] = "Not quite yet";
            TempData["ToastMessage"] = "Review the hint and try again.";
            TempData["ToastIcon"] = "↻";
        }

        return RedirectToAction(nameof(Take), new { id });
    }

    private static string Normalize(string? value) =>
        (value ?? string.Empty).Trim().Replace(" ", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty);
}

