using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Dashboard;
using CodeRoom.Web.ViewModels.Profile;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class ProfileController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var activities = await db.UserActivities
            .Where(a => a.UserId == user.Id && a.CreatedAt >= DateTime.UtcNow.Date.AddDays(-364))
            .OrderBy(a => a.CreatedAt)
            .ToListAsync();

        var courses = await db.Enrollments
            .Where(e => e.UserId == user.Id)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .ToListAsync();

        var completedLessons = await db.Progress.CountAsync(p => p.UserId == user.Id && p.IsCompleted);
        var totalLessons = courses.Sum(c => c.Course.Lessons.Count);

        return View(new ProfileViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio ?? string.Empty,
            RoleLabel = user.Role,
            IsAdmin = user.Role is Roles.Admin or Roles.SuperAdmin,
            CoursesEnrolled = courses.Count,
            LessonsCompleted = completedLessons,
            QuizAttempts = await db.QuizAttempts.CountAsync(q => q.UserId == user.Id),
            ProgressPercent = totalLessons == 0 ? 0 : (int)Math.Round(completedLessons * 100.0 / totalLessons),
            LearningStreak = LearningActivityService.CalculateStreak(activities),
            ActivityDays = BuildActivityDays(activities),
            RecentActivity = activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new RecentActivityViewModel
                {
                    ActivityType = a.ActivityType,
                    Description = a.Description,
                    CreatedAt = a.CreatedAt
                })
                .ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        return View(new ProfileSettingsViewModel
        {
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Settings(ProfileSettingsViewModel model)
    {
        var user = await GetCurrentUserAsync();
        if (user is null)
        {
            return Challenge();
        }

        var username = model.Username.Trim().ToLowerInvariant();
        var email = model.Email.Trim().ToLowerInvariant();

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await db.Users.AnyAsync(u => u.Id != user.Id && u.Username == username))
        {
            ModelState.AddModelError(nameof(model.Username), "That username is already in use.");
        }

        if (await db.Users.AnyAsync(u => u.Id != user.Id && u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "That email address is already in use.");
        }

        var passwordChangeRequested =
            !string.IsNullOrWhiteSpace(model.NewPassword) ||
            !string.IsNullOrWhiteSpace(model.CurrentPassword) ||
            !string.IsNullOrWhiteSpace(model.ConfirmNewPassword);

        if (passwordChangeRequested)
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword) ||
                !PasswordHasher.Verify(model.CurrentPassword, user.PasswordHash))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Enter your current password to change it.");
            }

            if (string.IsNullOrWhiteSpace(model.NewPassword))
            {
                ModelState.AddModelError(nameof(model.NewPassword), "Enter a new password.");
            }

            if (!string.Equals(model.NewPassword, model.ConfirmNewPassword, StringComparison.Ordinal))
            {
                ModelState.AddModelError(nameof(model.ConfirmNewPassword), "The new passwords do not match.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var profileChanged =
            user.FullName != model.FullName.Trim() ||
            user.Username != username ||
            user.Email != email ||
            (user.Bio ?? string.Empty) != (model.Bio?.Trim() ?? string.Empty);

        user.FullName = model.FullName.Trim();
        user.Username = username;
        user.Email = email;
        user.Bio = model.Bio?.Trim();

        if (passwordChangeRequested && !string.IsNullOrWhiteSpace(model.NewPassword))
        {
            user.PasswordHash = PasswordHasher.Hash(model.NewPassword);
        }

        await db.SaveChangesAsync();

        if (profileChanged || passwordChangeRequested)
        {
            await LearningActivityService.RecordAsync(
                db,
                user.Id,
                "ProfileUpdated",
                passwordChangeRequested ? "Updated profile and password" : "Updated profile details",
                "Profile updated",
                passwordChangeRequested
                    ? "Your profile and password have been updated successfully."
                    : "Your profile details have been updated successfully.",
                "/Profile",
                "ProfileUpdated");
        }

        await LearningActivityService.TryRecordStreakMilestoneAsync(db, user.Id);
        await RefreshSignInAsync(user);

        TempData["ToastTitle"] = "Changes saved";
        TempData["ToastMessage"] = passwordChangeRequested
            ? "Your profile and password are up to date."
            : "Your profile details are up to date.";
        TempData["ToastIcon"] = "✓";

        return RedirectToAction(nameof(Index));
    }

    private async Task<User?> GetCurrentUserAsync() =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == User.GetUserId());

    private static List<ActivityDayViewModel> BuildActivityDays(IEnumerable<UserActivity> activities)
    {
        var counts = activities
            .GroupBy(a => a.CreatedAt.Date)
            .ToDictionary(g => g.Key, g => g.Count());

        var start = DateTime.UtcNow.Date.AddDays(-364);
        return Enumerable.Range(0, 365)
            .Select(offset =>
            {
                var date = start.AddDays(offset);
                var count = counts.GetValueOrDefault(date, 0);
                return new ActivityDayViewModel
                {
                    Date = date,
                    Count = count,
                    Level = count switch
                    {
                        0 => 0,
                        1 => 1,
                        <= 3 => 2,
                        <= 6 => 3,
                        _ => 4
                    }
                };
            })
            .ToList();
    }

    private async Task RefreshSignInAsync(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("username", user.Username)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = User.Identity?.IsAuthenticated ?? true,
                AllowRefresh = true
            });
    }
}
