using ECommerce.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ECommerce.Infrastructure.Data.Configurations;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.ConfigureBaseEntity();
        builder.Property(entity => entity.CustomerId).HasMaxLength(450).IsRequired();
        builder.Property(entity => entity.Comment).HasMaxLength(4000);
        builder.Property(entity => entity.ReviewedAt).IsRequired();
        builder.ToTable(table => table.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));
        builder.HasOne(entity => entity.Product).WithMany(entity => entity.Reviews)
            .HasForeignKey(entity => entity.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
