using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult About() => View();

    public IActionResult Faq() => View();

    public IActionResult Error() => View();
}
