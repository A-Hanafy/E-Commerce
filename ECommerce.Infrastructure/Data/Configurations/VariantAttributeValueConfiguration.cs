using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class VariantAttributeValueConfiguration : IEntityTypeConfiguration<VariantAttributeValue>
{
    public void Configure(EntityTypeBuilder<VariantAttributeValue> builder)
    {
        builder.ToTable("VariantAttributeValues");
        builder.HasKey(entity => new { entity.ProductVariantId, entity.AttributeValueId });
        builder.HasOne(entity => entity.ProductVariant).WithMany(entity => entity.VariantAttributeValues)
            .HasForeignKey(entity => entity.ProductVariantId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(entity => entity.AttributeValue).WithMany(entity => entity.VariantAttributeValues)
            .HasForeignKey(entity => entity.AttributeValueId).OnDelete(DeleteBehavior.Restrict);
    }
}
