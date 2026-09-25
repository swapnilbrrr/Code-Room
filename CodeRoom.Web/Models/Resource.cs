using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.Models;

public class Resource
{
    public int Id { get; set; }

    [Display(Name = "Course (optional)")]
    public int? CourseId { get; set; }

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "URL is required.")]
    [StringLength(400)]
    [Url(ErrorMessage = "Enter a valid URL.")]
    public string Url { get; set; } = string.Empty;

    [Required]
    [StringLength(40)]
    public string Type { get; set; } = "Document";
}
