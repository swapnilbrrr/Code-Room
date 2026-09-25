using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class InfoController : Controller
{
    public IActionResult About() => View();
    public IActionResult Faq() => View();
}
