using TestApi.Application.Interfaces;
using TestApi.Infrastructure.Data;

namespace TestApi.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public ICategoryRepository Categories { get; private set; }
    public IProductRepository Products { get; private set; }

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
        Categories = new CategoryRepository(context);
        Products = new ProductRepository(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
