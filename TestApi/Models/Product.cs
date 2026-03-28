namespace TestApi.Models;

public class Product
{
    public int Id { get; set; }                         // Primary Key
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? Description { get; set; }            // اختياري
    public int CategoryId { get; set; }                 // Foreign Key
    public Category Category { get; set; } = null!;     // Navigation
}