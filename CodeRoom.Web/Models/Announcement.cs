using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Announcement
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Message is required.")]
    [DataType(DataType.MultilineText)]
    [StringLength(1000)]
    public string Message { get; set; } = string.Empty;

    [Display(Name = "Published date")]
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Published")]
    public bool IsPublished { get; set; } = true;
}
