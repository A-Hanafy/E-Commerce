namespace ECommerce.Core.Entities;

public sealed class ProductVariant : BaseEntity
{
    public int ProductId { get; set; }

    public string SKU { get; set; } = string.Empty;

    public decimal? Price { get; set; }

    public int StockQuantity { get; set; }

    public Product Product { get; set; } = null!;

    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
}
