using CodeRoom.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CodeRoom.Web.Controllers;

public class CoursesController(CourseCatalog catalog) : Controller
{
    public IActionResult Index() => View(catalog.GetAll());

    public IActionResult Details(int id)
    {
        var course = catalog.GetById(id);
        return course is null ? NotFound() : View(course);
    }
}
