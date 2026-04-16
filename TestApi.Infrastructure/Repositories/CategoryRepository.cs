using Microsoft.EntityFrameworkCore;
using TestApi.Application.Interfaces;
using TestApi.Domain.Models;
using TestApi.Infrastructure.Data;

namespace TestApi.Infrastructure.Repositories;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetByIdWithProductsAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.CategoryId == id);
    }
}
