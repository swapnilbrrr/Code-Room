using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class StudentController : Controller
{
    public IActionResult Index() => View();
}
