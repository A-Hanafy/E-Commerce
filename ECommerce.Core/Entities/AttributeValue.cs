namespace ECommerce.Core.Entities;

public sealed class AttributeValue : BaseEntity
{
    public int AttributeId { get; set; }

    public string Value { get; set; } = string.Empty;

    public Attribute Attribute { get; set; } = null!;

    public ICollection<VariantAttributeValue> VariantAttributeValues { get; set; } = new List<VariantAttributeValue>();
}
