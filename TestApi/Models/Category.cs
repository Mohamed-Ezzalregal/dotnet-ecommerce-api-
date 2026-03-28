namespace TestApi.Models;

public class Category
{
    public int Id { get; set; }                        // Primary Key
    public string Name { get; set; } = string.Empty;   // اسم التصنيف
    public List<Product> Products { get; set; } = new(); // المنتجات التابعة ليه
}