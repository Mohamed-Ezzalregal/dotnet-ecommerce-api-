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
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoriesController> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAll()
    {
        var categories = await _unitOfWork.Categories.GetAllAsync();
        var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
        return Ok(categoriesDto);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) throw new NotFoundException("Category not found");
        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryDto dto)
    {
        var category = _mapper.Map<Category>(dto);
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Category created: {CategoryId} - {CategoryName}", category.Id, category.Name);
        var categoryDto = _mapper.Map<CategoryDto>(category);
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, categoryDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, CreateCategoryDto dto)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) throw new NotFoundException("Category not found");
        _mapper.Map(dto, category);
        _unitOfWork.Categories.Update(category);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Category updated: {CategoryId} - {CategoryName}", category.Id, category.Name);
        var categoryDto = _mapper.Map<CategoryDto>(category);
        return Ok(categoryDto);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(id);
        if (category is null) throw new NotFoundException("Category not found");

        if (await _unitOfWork.Categories.HasProductsAsync(id))
            throw new BadRequestException("Cannot delete category that has products. Remove the products first.");

        _unitOfWork.Categories.Remove(category);
        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("Category deleted: {CategoryId}", id);
        return Ok("Deleted!");
    }
}