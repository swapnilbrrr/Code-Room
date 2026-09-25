using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Quizzes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class QuizController(ApplicationDbContext db) : Controller
{
    // id = quiz id
    public async Task<IActionResult> Index(int id)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);

        if (quiz is null || quiz.Questions.Count == 0)
        {
            return NotFound();
        }

        return View(new TakeQuizViewModel
        {
            Quiz = quiz,
            Questions = quiz.Questions.ToList()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int quizId, Dictionary<int, string>? answers)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == quizId);

        if (quiz is null || quiz.Questions.Count == 0)
        {
            return NotFound();
        }

        answers ??= [];
        var score = quiz.Questions.Count(q =>
            answers.TryGetValue(q.Id, out var chosen) &&
            string.Equals(chosen, q.CorrectOption, StringComparison.OrdinalIgnoreCase));

        var attempt = new QuizAttempt
        {
            UserId = User.GetUserId(),
            QuizId = quiz.Id,
            Score = score,
            TotalQuestions = quiz.Questions.Count,
            AttemptedAt = DateTime.UtcNow
        };

        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return RedirectToAction(nameof(Results), new { id = attempt.Id });
    }

    // id = attempt id
    public async Task<IActionResult> Results(int id)
    {
        var attempt = await db.QuizAttempts
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == id && a.UserId == User.GetUserId());

        if (attempt is null)
        {
            return NotFound();
        }

        return View(new QuizResultViewModel
        {
            QuizTitle = attempt.Quiz.Title,
            CourseId = attempt.Quiz.CourseId,
            Score = attempt.Score,
            Total = attempt.TotalQuestions
        });
    }
}
