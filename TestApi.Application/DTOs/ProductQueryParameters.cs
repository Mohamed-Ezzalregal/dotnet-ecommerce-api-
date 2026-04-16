using System.ComponentModel.DataAnnotations;

namespace TestApi.Application.DTOs;

public class ProductQueryParameters
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1")]
    public int Page { get; set; } = 1;

    [Range(1, 50, ErrorMessage = "PageSize must be between 1 and 50")]
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    public string? SortBy { get; set; }

    public bool SortDescending { get; set; } = false;
}
