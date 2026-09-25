using System.Security.Claims;

namespace CodeRoom.Web.Services;

/// <summary>Central definition of the platform roles used for authorization.</summary>
public static class Roles
{
    public const string Student = "Student";
    public const string Admin = "Admin";
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>Roles allowed into the /Admin area.</summary>
    public const string AdminArea = Admin + "," + SuperAdmin;
}

public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var id) ? id : 0;
    }

    public static string GetDisplayName(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(ClaimTypes.Name) ?? "Learner";
}
