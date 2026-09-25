using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class DashboardController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Quiz() => View();
    public IActionResult Results() => View();
}
