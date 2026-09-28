namespace ECommerce.Core.Entities;

public sealed class Attribute : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
