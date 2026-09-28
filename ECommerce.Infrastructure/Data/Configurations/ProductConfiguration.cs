using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.ConfigureBaseEntity();
        builder.Property(entity => entity.Name).HasMaxLength(300).IsRequired();
        builder.Property(entity => entity.Description).HasMaxLength(4000);
        builder.Property(entity => entity.BasePrice).HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.MainImageUrl).HasMaxLength(2048);
        builder.HasOne(entity => entity.Category).WithMany(entity => entity.Products)
            .HasForeignKey(entity => entity.CategoryId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(entity => entity.Brand).WithMany(entity => entity.Products)
            .HasForeignKey(entity => entity.BrandId).OnDelete(DeleteBehavior.Restrict);
    }
}
