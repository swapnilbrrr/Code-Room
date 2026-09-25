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
public class QuizzesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var quizzes = await db.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .OrderBy(q => q.Title)
            .ToListAsync();
        return View(quizzes);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        ViewBag.Courses = await CourseSelectAsync(null);
        return View(new Quiz());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Quiz model)
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

        db.Quizzes.Add(model);
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Created", "Quiz", model.Title, $"Created assessment {model.Title}.");
        TempData["Success"] = "Quiz created. Now add questions.";
        return RedirectToAction(nameof(Manage), new { id = model.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var quiz = await db.Quizzes.FindAsync(id);
        if (quiz is null)
        {
            return NotFound();
        }
        ViewBag.Courses = await CourseSelectAsync(quiz.CourseId);
        return View(quiz);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Quiz model)
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

        var quiz = await db.Quizzes.FindAsync(id);
        if (quiz is null)
        {
            return NotFound();
        }
        quiz.CourseId = model.CourseId;
        quiz.Title = model.Title;
        quiz.Description = model.Description;
        quiz.AssessmentType = model.AssessmentType;
        quiz.TimeLimitMinutes = model.TimeLimitMinutes;
        quiz.PassingScorePercent = model.PassingScorePercent;
        quiz.IsCertificationExam = model.IsCertificationExam;
        await db.SaveChangesAsync();
        await AdminAuditService.RecordAsync(db, User.GetUserId(), "Updated", "Quiz", quiz.Title, $"Updated assessment {quiz.Title}.");
        TempData["Success"] = "Quiz updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var quiz = await db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id);
        if (quiz is not null)
        {
            var deletedTitle = quiz.Title;
            db.Questions.RemoveRange(quiz.Questions);
            db.Quizzes.Remove(quiz);
            await db.SaveChangesAsync();
            await AdminAuditService.RecordAsync(db, User.GetUserId(), "Deleted", "Quiz", deletedTitle, $"Deleted assessment {deletedTitle}.");
            TempData["Success"] = "Quiz deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // ---- Question management ----

    [HttpGet]
    public async Task<IActionResult> Manage(int id)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Course)
            .Include(q => q.Questions)
            .FirstOrDefaultAsync(q => q.Id == id);
        return quiz is null ? NotFound() : View(quiz);
    }

    [HttpGet]
    public async Task<IActionResult> CreateQuestion(int quizId)
    {
        if (!await db.Quizzes.AnyAsync(q => q.Id == quizId))
        {
            return NotFound();
        }
        return View(new Question { QuizId = quizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateQuestion(Question model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        db.Questions.Add(model);
        await db.SaveChangesAsync();
        TempData["Success"] = "Question added.";
        return RedirectToAction(nameof(Manage), new { id = model.QuizId });
    }

    [HttpGet]
    public async Task<IActionResult> EditQuestion(int id)
    {
        var question = await db.Questions.FindAsync(id);
        return question is null ? NotFound() : View(question);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditQuestion(int id, Question model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var question = await db.Questions.FindAsync(id);
        if (question is null)
        {
            return NotFound();
        }
        question.QuestionText = model.QuestionText;
        question.OptionA = model.OptionA;
        question.OptionB = model.OptionB;
        question.OptionC = model.OptionC;
        question.OptionD = model.OptionD;
        question.CorrectOption = model.CorrectOption;
        await db.SaveChangesAsync();
        TempData["Success"] = "Question updated.";
        return RedirectToAction(nameof(Manage), new { id = question.QuizId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteQuestion(int id)
    {
        var question = await db.Questions.FindAsync(id);
        if (question is not null)
        {
            var quizId = question.QuizId;
            db.Questions.Remove(question);
            await db.SaveChangesAsync();
            TempData["Success"] = "Question deleted.";
            return RedirectToAction(nameof(Manage), new { id = quizId });
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<SelectList> CourseSelectAsync(int? selected) =>
        new(await db.Courses.OrderBy(c => c.Title).ToListAsync(), "Id", "Title", selected);
}
