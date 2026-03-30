using System.ComponentModel.DataAnnotations;

namespace TestApi.DTOs
{
    public class CreateProductDto
    {
        [Required(ErrorMessage = "Product name is required")]
        [MinLength(2, ErrorMessage = "Name must be at least 2 characters")]
        [MaxLength(100, ErrorMessage = "Name must be less than 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Range(0.01, 100000, ErrorMessage = "Price must be between 0.01 and 100,000")]
        public decimal Price { get; set; }

        [MaxLength(500, ErrorMessage = "Description must be less than 500 characters")]
        public string? Description { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "CategoryId is required")]
        public int CategoryId { get; set; }
    }
}