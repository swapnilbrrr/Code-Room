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
public class ProfileController(ApplicationDbContext db, IWebHostEnvironment environment) : Controller
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

        var isAdmin = user.Role is Roles.Admin or Roles.SuperAdmin;
        var adminLogs = isAdmin
            ? await db.AdminAuditLogs
                .Where(a => a.UserId == user.Id && a.CreatedAt >= DateTime.UtcNow.Date.AddDays(-364))
                .OrderBy(a => a.CreatedAt)
                .ToListAsync()
            : [];

        var courses = await db.Enrollments
            .Where(e => e.UserId == user.Id)
            .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
            .ToListAsync();

        var completedLessons = await db.Progress.CountAsync(p => p.UserId == user.Id && p.IsCompleted);
        var totalLessons = courses.Sum(c => c.Course.Lessons.Count);
        var achievements = await db.UserAchievements
            .Where(x => x.UserId == user.Id)
            .Include(x => x.Achievement)
            .OrderByDescending(x => x.EarnedAt)
            .Select(x => x.Achievement)
            .Take(12)
            .ToListAsync();
        var certificateCount = await db.Certificates.CountAsync(x => x.UserId == user.Id);

        return View(new ProfileViewModel
        {
            UserId = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            RoleLabel = user.Role,
            IsAdmin = isAdmin,
            AdminManagedUsers = isAdmin ? await db.Users.CountAsync() : 0,
            AdminCourseCount = isAdmin ? await db.Courses.CountAsync() : 0,
            AdminAuditEvents = adminLogs.Count,
            AdminPublishedAnnouncements = isAdmin ? await db.Announcements.CountAsync(a => a.IsPublished) : 0,
            CoursesEnrolled = courses.Count,
            LessonsCompleted = completedLessons,
            QuizAttempts = await db.QuizAttempts.CountAsync(q => q.UserId == user.Id),
            ProgressPercent = totalLessons == 0 ? 0 : (int)Math.Round(completedLessons * 100.0 / totalLessons),
            LearningStreak = LearningActivityService.CalculateStreak(activities),
            Xp = user.Xp,
            Level = LearningActivityService.GetLevel(user.Xp),
            LevelProgress = LearningActivityService.GetLevelProgress(user.Xp),
            CertificateCount = certificateCount,
            Achievements = achievements,
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
                .ToList(),
            AdminRecentActivity = adminLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(8)
                .Select(a => new RecentActivityViewModel
                {
                    ActivityType = a.Action,
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
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            ThemePreference = user.ThemePreference,
            ProfileVisibility = user.ProfileVisibility,
            EmailNotificationsEnabled = user.EmailNotificationsEnabled
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
            (user.Bio ?? string.Empty) != (model.Bio?.Trim() ?? string.Empty) ||
            (user.AvatarUrl ?? string.Empty) != (model.AvatarUrl?.Trim() ?? string.Empty) ||
            user.ThemePreference != model.ThemePreference ||
            user.ProfileVisibility != model.ProfileVisibility ||
            user.EmailNotificationsEnabled != model.EmailNotificationsEnabled ||
            model.AvatarFile is not null;

        user.FullName = model.FullName.Trim();
        user.Username = username;
        user.Email = email;
        user.Bio = model.Bio?.Trim();
        if (model.AvatarFile is not null)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(model.AvatarFile.FileName).ToLowerInvariant();

            if (!allowed.Contains(extension) || model.AvatarFile.Length > 2 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.AvatarFile), "Avatar must be JPG, PNG or WebP and no larger than 2 MB.");
                return View(model);
            }

            var directory = Path.Combine(environment.WebRootPath, "uploads", "avatars");
            Directory.CreateDirectory(directory);

            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(directory, fileName);
            await using var stream = System.IO.File.Create(filePath);
            await model.AvatarFile.CopyToAsync(stream);

            if (!string.IsNullOrWhiteSpace(user.AvatarUrl) && user.AvatarUrl.StartsWith("/uploads/avatars/", StringComparison.OrdinalIgnoreCase))
            {
                var oldPath = Path.Combine(environment.WebRootPath, user.AvatarUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            user.AvatarUrl = $"/uploads/avatars/{fileName}";
        }
        else
        {
            user.AvatarUrl = model.AvatarUrl?.Trim();
        }
        user.ThemePreference = model.ThemePreference is "light" or "dark" ? model.ThemePreference : "system";
        user.ProfileVisibility = model.ProfileVisibility is "Public" or "Members" ? model.ProfileVisibility : "Public";
        user.EmailNotificationsEnabled = model.EmailNotificationsEnabled;

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
        var currentAuth = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        var isPersistent = currentAuth.Properties?.IsPersistent ?? true;

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = isPersistent,
                AllowRefresh = true
            });
    }
}
