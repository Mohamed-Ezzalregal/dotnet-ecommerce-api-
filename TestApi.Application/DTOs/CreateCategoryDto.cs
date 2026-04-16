using System.ComponentModel.DataAnnotations;

namespace TestApi.Application.DTOs;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
    [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
    public string Name { get; set; } = string.Empty;
}
