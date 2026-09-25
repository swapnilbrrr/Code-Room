namespace CodeRoom.Web.ViewModels;

public sealed class CoursePreviewViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string CategorySlug { get; init; } = string.Empty;
    public string Level { get; init; } = string.Empty;
    public string LevelSlug { get; init; } = string.Empty;
    public string Summary { get; init; } = string.Empty;
    public string[] LearningOutcomes { get; init; } = [];
    public string[] Lessons { get; init; } = [];
}
