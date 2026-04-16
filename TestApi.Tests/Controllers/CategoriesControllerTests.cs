using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TestApi.Application.DTOs;
using TestApi.Application.Exceptions;
using TestApi.Application.Interfaces;
using TestApi.Application.Mapping;
using TestApi.Controllers;
using TestApi.Domain.Models;

namespace TestApi.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<CategoriesController>> _loggerMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<CategoriesController>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new CategoriesController(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    // ===== GetAll Tests =====

    [Fact]
    public async Task GetAll_ReturnsAllCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Phones" },
            new Category { Id = 2, Name = "Laptops" }
        };

        _unitOfWorkMock.Setup(u => u.Categories.GetAllAsync())
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categoriesDto = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject;
        categoriesDto.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAll_ReturnsEmpty_WhenNoCategories()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Categories.GetAllAsync())
            .ReturnsAsync(new List<Category>());

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categoriesDto = okResult.Value.Should().BeAssignableTo<IEnumerable<CategoryDto>>().Subject;
        categoriesDto.Should().BeEmpty();
    }

    // ===== GetById Tests =====

    [Fact]
    public async Task GetById_ReturnsCategory_WhenExists()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Phones" };
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categoryDto = okResult.Value.Should().BeOfType<CategoryDto>().Subject;
        categoryDto.Id.Should().Be(1);
        categoryDto.Name.Should().Be("Phones");
    }

    [Fact]
    public async Task GetById_ThrowsNotFoundException_WhenNotExists()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var act = () => _controller.GetById(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category not found");
    }

    // ===== Create Tests =====

    [Fact]
    public async Task Create_ReturnsCreatedCategory_WhenValid()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Tablets" };

        _unitOfWorkMock.Setup(u => u.Categories.AddAsync(It.IsAny<Category>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var categoryDto = createdResult.Value.Should().BeOfType<CategoryDto>().Subject;
        categoryDto.Name.Should().Be("Tablets");
    }

    // ===== Update Tests =====

    [Fact]
    public async Task Update_ReturnsUpdatedCategory_WhenValid()
    {
        // Arrange
        var existingCategory = new Category { Id = 1, Name = "Old Name" };
        var dto = new CreateCategoryDto { Name = "New Name" };

        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(existingCategory);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var categoryDto = okResult.Value.Should().BeOfType<CategoryDto>().Subject;
        categoryDto.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenCategoryNotExists()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Test" };
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var act = () => _controller.Update(999, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category not found");
    }

    // ===== Delete Tests =====

    [Fact]
    public async Task Delete_ReturnsOk_WhenCategoryExists_AndNoProducts()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Phones" };
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.Categories.HasProductsAsync(1))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _unitOfWorkMock.Verify(u => u.Categories.Remove(category), Times.Once);
    }

    [Fact]
    public async Task Delete_ThrowsBadRequest_WhenCategoryHasProducts()
    {
        // Arrange
        var category = new Category { Id = 1, Name = "Phones" };
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.Categories.HasProductsAsync(1))
            .ReturnsAsync(true);

        // Act
        var act = () => _controller.Delete(1);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*products*");
    }

    [Fact]
    public async Task Delete_ThrowsNotFoundException_WhenCategoryNotExists()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var act = () => _controller.Delete(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Category not found");
    }
}
