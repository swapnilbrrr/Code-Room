using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

}
