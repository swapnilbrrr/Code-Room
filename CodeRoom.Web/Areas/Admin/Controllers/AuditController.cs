using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.AdminArea)]
public class AuditController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var logs = await db.AdminAuditLogs
            .Include(x => x.User)
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .ToListAsync();

        return View(logs);
    }
}
