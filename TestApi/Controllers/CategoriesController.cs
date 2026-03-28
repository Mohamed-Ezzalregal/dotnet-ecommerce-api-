using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApi.Data;
using TestApi.DTOs;
using TestApi.Models;

namespace TestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Category>>> GetAll()
    {
        return Ok(await _db.Categories.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Category>> GetById(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound("Category not found");
        return Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<Category>> Create(CreateCategoryDto dto)
    {
        var category = new Category { Name = dto.Name };
        _db.Categories.Add(category);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, CreateCategoryDto dto)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound("Category not found");
        category.Name = dto.Name;
        await _db.SaveChangesAsync();
        return Ok(category);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _db.Categories.FindAsync(id);
        if (category is null) return NotFound("Category not found");
        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return Ok("Deleted!");
    }
}