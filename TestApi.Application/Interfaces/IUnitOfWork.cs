namespace TestApi.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    Task<int> SaveChangesAsync();
}
