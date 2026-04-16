using Microsoft.EntityFrameworkCore;
using TestApi.Application.DTOs;
using TestApi.Application.Interfaces;
using TestApi.Domain.Models;
using TestApi.Infrastructure.Data;

namespace TestApi.Infrastructure.Repositories;

public class ProductRepository : Repository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetAllWithCategoryAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(IEnumerable<Product> Products, int TotalCount)> GetFilteredAsync(ProductQueryParameters query)
    {
        var products = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            products = products.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.Description != null && p.Description.ToLower().Contains(search)));
        }

        if (query.CategoryId.HasValue)
        {
            products = products.Where(p => p.CategoryId == query.CategoryId.Value);
        }

        if (query.MinPrice.HasValue)
        {
            products = products.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            products = products.Where(p => p.Price <= query.MaxPrice.Value);
        }

        products = query.SortBy?.ToLower() switch
        {
            "name" => query.SortDescending
                ? products.OrderByDescending(p => p.Name)
                : products.OrderBy(p => p.Name),
            "price" => query.SortDescending
                ? products.OrderByDescending(p => p.Price)
                : products.OrderBy(p => p.Price),
            _ => query.SortDescending
                ? products.OrderByDescending(p => p.Id)
                : products.OrderBy(p => p.Id)
        };

        var totalCount = await products.CountAsync();

        var items = await products
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
