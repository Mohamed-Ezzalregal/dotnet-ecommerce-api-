using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TestApi.Application.DTOs;
using TestApi.Application.Exceptions;
using TestApi.Application.Interfaces;
using TestApi.Domain.Models;

namespace TestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProductDto>>> GetAll([FromQuery] ProductQueryParameters query)
    {
        var (products, totalCount) = await _unitOfWork.Products.GetFilteredAsync(query);
        var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);

        var result = new PagedResult<ProductDto>
        {
            Items = productsDto,
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetById(int id)
    {
        var product = await _unitOfWork.Products.GetByIdWithCategoryAsync(id);
        if (product is null) throw new NotFoundException("Product not found");
        var productDto = _mapper.Map<ProductDto>(product);
        return Ok(productDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(CreateProductDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (category is null) throw new BadRequestException("Category not found. Please provide a valid CategoryId.");

        var product = _mapper.Map<Product>(dto);
        await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product created: {ProductId} - {ProductName}", product.Id, product.Name);

        var created = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
        var productDto = _mapper.Map<ProductDto>(created);
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, productDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> Update(int id, CreateProductDto dto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null) throw new NotFoundException("Product not found");

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
        if (category is null) throw new BadRequestException("Category not found. Please provide a valid CategoryId.");

        _mapper.Map(dto, product);
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product updated: {ProductId} - {ProductName}", product.Id, product.Name);

        var updated = await _unitOfWork.Products.GetByIdWithCategoryAsync(product.Id);
        var productDto = _mapper.Map<ProductDto>(updated);
        return Ok(productDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product is null) throw new NotFoundException("Product not found");
        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Product deleted: {ProductId}", id);
        return Ok("Deleted!");
    }
}