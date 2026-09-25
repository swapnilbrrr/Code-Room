using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Areas.Admin.Controllers;

[Area("Admin")]
public class DashboardController : Controller
{
    public IActionResult Index() => View();
}
