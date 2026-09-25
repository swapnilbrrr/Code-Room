using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class LessonsController : Controller
{
    public IActionResult Index(int id) => View(id);
}
