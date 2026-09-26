using CodeRoom.Web.Data;
using CodeRoom.Web.Models;
using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Roles.SuperAdmin)]
public class SystemController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        ViewBag.UserCount = await db.Users.CountAsync();
        ViewBag.AdminCount = await db.Users.CountAsync(u => u.Role == Roles.Admin);
        ViewBag.SuperAdminCount = await db.Users.CountAsync(u => u.Role == Roles.SuperAdmin);
        ViewBag.CourseCount = await db.Courses.CountAsync();
        ViewBag.CertificateCount = await db.Certificates.CountAsync();
        ViewBag.AuditCount = await db.AdminAuditLogs.CountAsync();

        return View();
    }
}
