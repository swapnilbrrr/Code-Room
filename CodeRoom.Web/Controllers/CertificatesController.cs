using CodeRoom.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CodeRoom.Web.Controllers;

[Authorize]
public class CertificatesController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var certificates = await db.Certificates
            .Where(c => c.UserId == User.GetUserId())
            .Include(c => c.Course)
            .OrderByDescending(c => c.IssuedAt)
            .ToListAsync();

        return View(certificates);
    }

    public async Task<IActionResult> Details(int id)
    {
        var certificate = await db.Certificates
            .Include(c => c.Course)
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id && c.UserId == User.GetUserId());

        return certificate is null ? NotFound() : View(certificate);
    }
}
