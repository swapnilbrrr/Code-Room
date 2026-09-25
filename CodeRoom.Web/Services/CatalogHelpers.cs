namespace CodeRoom.Web.Services;

/// <summary>Small presentation helpers that map catalogue text to filter slugs.</summary>
public static class CatalogHelpers
{
    public static string CategorySlug(string category) => category switch
    {
        "Programming" => "programming",
        "Web Development" => "web",
        "Cybersecurity" => "security",
        "Databases" => "database",
        _ => "other"
    };

    public static string LevelSlug(string level) => level.ToLowerInvariant() switch
    {
        "beginner" => "beginner",
        "intermediate" => "intermediate",
        "advanced" => "advanced",
        _ => "beginner"
    };
}
