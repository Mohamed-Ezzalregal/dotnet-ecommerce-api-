using AutoMapper;
using TestApi.Application.DTOs;
using TestApi.Domain.Models;

namespace TestApi.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.CategoryName,
                       opt => opt.MapFrom(src => src.Category.Name));

        CreateMap<CreateProductDto, Product>();

        CreateMap<Category, CategoryDto>();

        CreateMap<CreateCategoryDto, Category>();

        CreateMap<RegisterDto, User>();
    }
}
