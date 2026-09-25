using CodeRoom.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CodeRoom.Web.Tests;

public class SmokeTests
{
    [Fact]
    public void HomeController_ReturnsHomeView()
    {
        var controller = new HomeController();

        var result = controller.Index();

        Assert.IsType<ViewResult>(result);
    }
}
