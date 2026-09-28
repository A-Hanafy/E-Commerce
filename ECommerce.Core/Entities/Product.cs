namespace ECommerce.Core.Entities;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal BasePrice { get; set; }

    public int CategoryId { get; set; }

    public int BrandId { get; set; }

    public string? MainImageUrl { get; set; }

    public Category Category { get; set; } = null!;

    public Brand Brand { get; set; } = null!;

    public ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
