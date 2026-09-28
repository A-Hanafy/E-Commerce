using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.ToTable("AttributeValues");
        builder.ConfigureBaseEntity();
        builder.Property(entity => entity.Value).HasMaxLength(200).IsRequired();
        builder.HasIndex(entity => new { entity.AttributeId, entity.Value }).IsUnique();
        builder.HasOne(entity => entity.Attribute).WithMany(entity => entity.AttributeValues)
            .HasForeignKey(entity => entity.AttributeId).OnDelete(DeleteBehavior.Cascade);
    }
}
