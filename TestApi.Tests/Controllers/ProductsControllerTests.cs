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

public class ProductsControllerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<ProductsController>> _loggerMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ProductsController>>();

        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _mapper = config.CreateMapper();

        _controller = new ProductsController(_unitOfWorkMock.Object, _mapper, _loggerMock.Object);
    }

    // ===== GetAll Tests =====

    [Fact]
    public async Task GetAll_ReturnsPagedResult_WithProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "iPhone", Price = 999, CategoryId = 1, Category = new Category { Id = 1, Name = "Phones" } },
            new Product { Id = 2, Name = "Samsung", Price = 899, CategoryId = 1, Category = new Category { Id = 1, Name = "Phones" } }
        };
        var query = new ProductQueryParameters { Page = 1, PageSize = 10 };

        _unitOfWorkMock.Setup(u => u.Products.GetFilteredAsync(query))
            .ReturnsAsync((products.AsEnumerable(), 2));

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().HaveCount(2);
        pagedResult.TotalCount.Should().Be(2);
        pagedResult.Page.Should().Be(1);
    }

    [Fact]
    public async Task GetAll_ReturnsEmptyResult_WhenNoProducts()
    {
        // Arrange
        var query = new ProductQueryParameters { Page = 1, PageSize = 10 };
        _unitOfWorkMock.Setup(u => u.Products.GetFilteredAsync(query))
            .ReturnsAsync((Enumerable.Empty<Product>(), 0));

        // Act
        var result = await _controller.GetAll(query);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var pagedResult = okResult.Value.Should().BeOfType<PagedResult<ProductDto>>().Subject;
        pagedResult.Items.Should().BeEmpty();
        pagedResult.TotalCount.Should().Be(0);
    }

    // ===== GetById Tests =====

    [Fact]
    public async Task GetById_ReturnsProduct_WhenExists()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Name = "iPhone",
            Price = 999,
            CategoryId = 1,
            Category = new Category { Id = 1, Name = "Phones" }
        };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetById(1);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Id.Should().Be(1);
        productDto.Name.Should().Be("iPhone");
        productDto.CategoryName.Should().Be("Phones");
    }

    [Fact]
    public async Task GetById_ThrowsNotFoundException_WhenNotExists()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _controller.GetById(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product not found");
    }

    // ===== Create Tests =====

    [Fact]
    public async Task Create_ReturnsCreatedProduct_WhenValid()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "MacBook",
            Price = 1999,
            Description = "Apple laptop",
            CategoryId = 1
        };

        var category = new Category { Id = 1, Name = "Laptops" };
        var createdProduct = new Product
        {
            Id = 5,
            Name = "MacBook",
            Price = 1999,
            Description = "Apple laptop",
            CategoryId = 1,
            Category = category
        };

        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.Products.AddAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(It.IsAny<int>()))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.Create(dto);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        var productDto = createdResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Name.Should().Be("MacBook");
        productDto.Price.Should().Be(1999);
        productDto.CategoryName.Should().Be("Laptops");
    }

    [Fact]
    public async Task Create_ThrowsBadRequest_WhenCategoryNotFound()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            Name = "MacBook",
            Price = 1999,
            CategoryId = 999
        };

        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var act = () => _controller.Create(dto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*CategoryId*");
    }

    // ===== Update Tests =====

    [Fact]
    public async Task Update_ReturnsUpdatedProduct_WhenValid()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 1,
            Name = "Old Name",
            Price = 500,
            CategoryId = 1
        };
        var category = new Category { Id = 1, Name = "Phones" };
        var dto = new CreateProductDto
        {
            Name = "New Name",
            Price = 600,
            CategoryId = 1
        };
        var updatedProduct = new Product
        {
            Id = 1,
            Name = "New Name",
            Price = 600,
            CategoryId = 1,
            Category = category
        };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(1))
            .ReturnsAsync(category);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);
        _unitOfWorkMock.Setup(u => u.Products.GetByIdWithCategoryAsync(1))
            .ReturnsAsync(updatedProduct);

        // Act
        var result = await _controller.Update(1, dto);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var productDto = okResult.Value.Should().BeOfType<ProductDto>().Subject;
        productDto.Name.Should().Be("New Name");
        productDto.Price.Should().Be(600);
    }

    [Fact]
    public async Task Update_ThrowsNotFoundException_WhenProductNotExists()
    {
        // Arrange
        var dto = new CreateProductDto { Name = "Test", Price = 100, CategoryId = 1 };
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _controller.Update(999, dto);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product not found");
    }

    [Fact]
    public async Task Update_ThrowsBadRequest_WhenCategoryNotFound()
    {
        // Arrange
        var existingProduct = new Product { Id = 1, Name = "Test", Price = 100, CategoryId = 1 };
        var dto = new CreateProductDto { Name = "Test", Price = 100, CategoryId = 999 };

        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
            .ReturnsAsync(existingProduct);
        _unitOfWorkMock.Setup(u => u.Categories.GetByIdAsync(999))
            .ReturnsAsync((Category?)null);

        // Act
        var act = () => _controller.Update(1, dto);

        // Assert
        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("*CategoryId*");
    }

    // ===== Delete Tests =====

    [Fact]
    public async Task Delete_ReturnsOk_WhenProductExists()
    {
        // Arrange
        var product = new Product { Id = 1, Name = "iPhone", Price = 999 };
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(1))
            .ReturnsAsync(product);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _controller.Delete(1);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _unitOfWorkMock.Verify(u => u.Products.Remove(product), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_ThrowsNotFoundException_WhenProductNotExists()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.Products.GetByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var act = () => _controller.Delete(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product not found");
    }
}
