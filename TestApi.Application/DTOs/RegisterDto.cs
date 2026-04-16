using System.ComponentModel.DataAnnotations;

namespace TestApi.Application.DTOs;

public class RegisterDto
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(30, ErrorMessage = "Username must be less than 30 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username can only contain letters, numbers, and underscores")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100, ErrorMessage = "Password must be less than 100 characters")]
    public string Password { get; set; } = string.Empty;
}
