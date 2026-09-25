using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class CoursesController : Controller
{
    public IActionResult Index() => View();
    public IActionResult Details(int id) => View(id);
    public IActionResult Lesson(int id) => View(id);
}
