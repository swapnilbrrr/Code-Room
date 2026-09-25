namespace CodeRoom.Web.Models;

public class Resource
{
    public int Id { get; set; }
    public int? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Type { get; set; } = "Document";
}
