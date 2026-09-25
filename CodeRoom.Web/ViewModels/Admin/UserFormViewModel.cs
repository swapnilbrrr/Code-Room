using System.ComponentModel.DataAnnotations;

namespace CodeRoom.Web.ViewModels.Admin;

public class UserFormViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(80, MinimumLength = 3)]
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress]
    [StringLength(120)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "Student";

    // On create: required. On edit: optional (blank keeps existing password).
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
    public string? Password { get; set; }

    public bool IsEdit { get; set; }
}
