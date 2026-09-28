using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("ProductVariants");
        builder.ConfigureBaseEntity();
        builder.Property(entity => entity.SKU).HasMaxLength(100).IsRequired();
        builder.Property(entity => entity.Price).HasPrecision(18, 2);
        builder.Property(entity => entity.StockQuantity).IsRequired();
        builder.HasIndex(entity => entity.SKU).IsUnique();
        builder.HasOne(entity => entity.Product).WithMany(entity => entity.ProductVariants)
            .HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
