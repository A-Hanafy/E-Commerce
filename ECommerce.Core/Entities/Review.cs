namespace ECommerce.Core.Entities;

public sealed class Review : BaseEntity
{
    public int ProductId { get; set; }

    public string CustomerId { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime ReviewedAt { get; set; }

    public Product Product { get; set; } = null!;
}
