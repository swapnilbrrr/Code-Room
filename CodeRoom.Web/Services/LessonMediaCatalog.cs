namespace CodeRoom.Web.Services;

public static class LessonMediaCatalog
{
    public static string? VideoFor(string courseTitle) => courseTitle switch
    {
        "C# Fundamentals" => "https://www.youtube.com/embed/GhQdlIFylQ8",
        "Python Programming" => "https://www.youtube.com/embed/rfscVS0vtbw",
        "HTML & CSS Foundations" => "https://www.youtube.com/embed/a_iQb1lnAEQ",
        "ASP.NET Core MVC" => "https://www.youtube.com/embed/6SAFgcMie4U",
        "Linux Fundamentals" => "https://www.youtube.com/embed/pkZEKIXe3u4",
        "Cybersecurity Foundations" => "https://www.youtube.com/embed/Q_hwxazyXQY",
        "Database Fundamentals" => "https://www.youtube.com/embed/HXV3zeQKqGY",
        _ => null
    };

    public static string? ResourceFor(string courseTitle) => courseTitle switch
    {
        "C# Fundamentals" => "https://learn.microsoft.com/dotnet/csharp/",
        "Python Programming" => "https://docs.python.org/3/tutorial/",
        "HTML & CSS Foundations" => "https://developer.mozilla.org/en-US/docs/Learn",
        "ASP.NET Core MVC" => "https://learn.microsoft.com/aspnet/core/",
        "Linux Fundamentals" => "https://linuxjourney.com/",
        "Cybersecurity Foundations" => "https://owasp.org/www-project-top-ten/",
        "Database Fundamentals" => "https://dev.mysql.com/doc/",
        _ => null
    };
}
