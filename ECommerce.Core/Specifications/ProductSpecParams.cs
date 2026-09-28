namespace ECommerce.Core.Specifications;

public sealed class ProductSpecParams
{
    private const int MaxPageSize = 50;
    private int _pageIndex = 1;
    private int _pageSize = 6;
    private string? _search;

    public int PageIndex
    {
        get => _pageIndex;
        set => _pageIndex = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value switch
        {
            < 1 => 6,
            > MaxPageSize => MaxPageSize,
            _ => value
        };
    }

    public int? BrandId { get; set; }

    public int? CategoryId { get; set; }

    public string? Sort { get; set; }

    public string? Search
    {
        get => _search;
        set => _search = value?.Trim().ToLowerInvariant();
    }
}
