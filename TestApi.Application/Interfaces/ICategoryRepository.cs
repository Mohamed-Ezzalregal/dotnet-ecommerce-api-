using TestApi.Domain.Models;

namespace TestApi.Application.Interfaces;

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetByIdWithProductsAsync(int id);
    Task<bool> HasProductsAsync(int id);
}
