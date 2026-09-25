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

        var percentage = quiz.Questions.Count == 0 ? 0 : (int)Math.Round(score * 100.0 / quiz.Questions.Count);
        var passed = quiz.AssessmentType == "Quiz"
            ? percentage >= 70
            : percentage >= quiz.PassingScorePercent;

        await LearningActivityService.RecordAsync(
            db,
            User.GetUserId(),
            passed ? "ExamPassed" : "QuizAttempted",
            $"Completed {quiz.Title} with {percentage}%",
            passed ? "Assessment passed" : "Quiz completed",
            $"You scored {score}/{quiz.Questions.Count} ({percentage}%) in {quiz.Title}.",
            $"/Quiz/Results/{attempt.Id}",
            passed ? "ExamPassed" : "QuizCompleted",
            passed && quiz.IsCertificationExam ? 150 : null);

        await LearningActivityService.TryRecordStreakMilestoneAsync(db, User.GetUserId());


        if (passed && quiz.IsCertificationExam)
        {
            var alreadyCertified = await db.Certificates.AnyAsync(c =>
                c.UserId == User.GetUserId() && c.CourseId == quiz.CourseId);

            if (!alreadyCertified)
            {
                var course = await db.Courses.FirstAsync(c => c.Id == quiz.CourseId);
                var certificate = new Certificate
                {
                    UserId = User.GetUserId(),
                    CourseId = course.Id,
                    QuizAttemptId = attempt.Id,
                    CertificateNumber = $"CR-{course.Id:D3}-{User.GetUserId():D3}-{attempt.Id:D5}",
                    Title = course.CertificateName ?? $"{course.Title} Certificate",
                    IssuedAt = DateTime.UtcNow
                };
                db.Certificates.Add(certificate);
                await db.SaveChangesAsync();

                await LearningActivityService.RecordAsync(
                    db,
                    User.GetUserId(),
                    "CertificateEarned",
                    $"Earned {certificate.Title}",
                    "Certificate unlocked",
                    "You passed the certification examination and earned a Code-Room certificate.",
                    $"/Certificates/Details/{certificate.Id}",
                    "Certificate",
                    200);
            }
        }

        TempData["ToastTitle"] = "Quiz submitted";
        TempData["ToastMessage"] = $"You scored {percentage}% in {quiz.Title}.";
        TempData["ToastIcon"] = percentage >= 80 ? "🏆" : "✓";

        return RedirectToAction(nameof(Results), new { id = attempt.Id });
    }

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
            Total = attempt.TotalQuestions,
            AssessmentType = attempt.Quiz.AssessmentType,
            PassingScorePercent = attempt.Quiz.PassingScorePercent,
            IsCertificationExam = attempt.Quiz.IsCertificationExam,
            Certificate = await db.Certificates.FirstOrDefaultAsync(c => c.UserId == User.GetUserId() && c.QuizAttemptId == attempt.Id)
        });
    }
}
