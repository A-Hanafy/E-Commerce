using CatalogAttribute = ECommerce.Core.Entities.Attribute;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class AttributeConfiguration : IEntityTypeConfiguration<CatalogAttribute>
{
    public void Configure(EntityTypeBuilder<CatalogAttribute> builder)
    {
        builder.ToTable("Attributes");
        builder.ConfigureBaseEntity();
        builder.Property(entity => entity.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(entity => entity.Name).IsUnique();
    }
}
