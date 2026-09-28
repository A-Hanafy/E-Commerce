namespace ECommerce.API.DTOs;

public sealed class ProductToReturnDto
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public decimal Price { get; init; }

    public string PictureUrl { get; init; } = string.Empty;

    public string ProductType { get; init; } = string.Empty;

    public string ProductBrand { get; init; } = string.Empty;
}
