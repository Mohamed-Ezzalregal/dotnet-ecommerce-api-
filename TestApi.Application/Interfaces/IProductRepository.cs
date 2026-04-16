using TestApi.Application.DTOs;
using TestApi.Domain.Models;

namespace TestApi.Application.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    Task<IEnumerable<Product>> GetAllWithCategoryAsync();
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredAsync(ProductQueryParameters query);
}
