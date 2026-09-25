using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class QuizController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Results() => View();
}
