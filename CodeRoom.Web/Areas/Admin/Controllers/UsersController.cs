using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using CodeRoom.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class UsersController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var users = await db.Users.OrderBy(u => u.FullName).ToListAsync();
        return View(users);
    }

    [HttpGet]
    public IActionResult Create() => View(new UserFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Password))
        {
            ModelState.AddModelError(nameof(model.Password), "Password is required for a new user.");
        }

        var email = model.Email.Trim().ToLowerInvariant();
        var username = model.Username.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        if (await db.Users.AnyAsync(u => u.Username == username))
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already in use.");
        }

        if (!IsRoleAllowed(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Only a Super Administrator can grant that role.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        db.Users.Add(new User
        {
            FullName = model.FullName.Trim(),
            Username = username,
            Email = email,
            Role = model.Role,
            PasswordHash = PasswordHasher.Hash(model.Password!)
        });
        await db.SaveChangesAsync();
        TempData["Success"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return View(new UserFormViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role,
            IsEdit = true
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }
        model.IsEdit = true;

        var email = model.Email.Trim().ToLowerInvariant();
        var username = model.Username.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != id))
        {
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");
        }

        if (await db.Users.AnyAsync(u => u.Username == username && u.Id != id))
        {
            ModelState.AddModelError(nameof(model.Username), "This username is already in use.");
        }

        if (!IsRoleAllowed(model.Role))
        {
            ModelState.AddModelError(nameof(model.Role), "Only a Super Administrator can grant that role.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.FullName = model.FullName.Trim();
        user.Username = username;
        user.Email = email;
        user.Role = model.Role;

        if (!string.IsNullOrWhiteSpace(model.Password))
        {
            user.PasswordHash = PasswordHasher.Hash(model.Password);
        }

        await db.SaveChangesAsync();
        TempData["Success"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        if (id == User.GetUserId())
        {
            TempData["Error"] = "You cannot delete your own account.";
            return RedirectToAction(nameof(Index));
        }

        var user = await db.Users.FindAsync(id);
        if (user is null)
        {
            return RedirectToAction(nameof(Index));
        }

        if (user.Role == Roles.SuperAdmin && !User.IsInRole(Roles.SuperAdmin))
        {
            TempData["Error"] = "Only a Super Administrator can delete that account.";
            return RedirectToAction(nameof(Index));
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync();
        TempData["Success"] = "User deleted.";
        return RedirectToAction(nameof(Index));
    }

    // Only a SuperAdmin may create/assign the SuperAdmin role.
    private bool IsRoleAllowed(string role) =>
        role != Roles.SuperAdmin || User.IsInRole(Roles.SuperAdmin);
}
