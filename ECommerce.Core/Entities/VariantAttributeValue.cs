namespace ECommerce.Core.Entities;

public sealed class VariantAttributeValue
{
    public int ProductVariantId { get; set; }

    public int AttributeValueId { get; set; }

    public ProductVariant ProductVariant { get; set; } = null!;

    public AttributeValue AttributeValue { get; set; } = null!;
}
