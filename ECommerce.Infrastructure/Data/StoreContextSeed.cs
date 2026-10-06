using ECommerce.Core.Entities;
using ECommerce.Core.Entities.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Data;

public static class StoreContextSeed
{
    public static async Task SeedAsync(AppDbContext context, ILoggerFactory loggerFactory)
    {
        var logger = loggerFactory.CreateLogger(typeof(StoreContextSeed));

        try
        {
            if (!await context.Categories.AnyAsync())
            {
                await context.Categories.AddRangeAsync(
                    new Category
                    {
                        Name = "Electronics",
                        Description = "Devices and electronic accessories.",
                        ImageUrl = "https://images.unsplash.com/photo-1498049794561-7780e7231661"
                    },
                    new Category
                    {
                        Name = "Clothes",
                        Description = "Everyday apparel and activewear.",
                        ImageUrl = "https://images.unsplash.com/photo-1489987707025-afc232f7ea0f"
                    },
                    new Category
                    {
                        Name = "Shoes",
                        Description = "Footwear for sports and daily wear.",
                        ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff"
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.Brands.AnyAsync())
            {
                await context.Brands.AddRangeAsync(
                    new Brand
                    {
                        Name = "Apple",
                        Description = "Consumer electronics and software.",
                        LogoUrl = "https://images.unsplash.com/photo-1621768216002-5ac171876625"
                    },
                    new Brand
                    {
                        Name = "Nike",
                        Description = "Sportswear and athletic footwear.",
                        LogoUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff"
                    },
                    new Brand
                    {
                        Name = "Adidas",
                        Description = "Sportswear, shoes, and accessories.",
                        LogoUrl = "https://images.unsplash.com/photo-1518002171953-a080ee817e1f"
                    });

                await context.SaveChangesAsync();
            }

            if (!await context.Products.AnyAsync())
            {
                var categories = await context.Categories
                    .Where(category => category.Name == "Electronics" || category.Name == "Clothes" || category.Name == "Shoes")
                    .ToDictionaryAsync(category => category.Name, category => category.Id);
                var brands = await context.Brands
                    .Where(brand => brand.Name == "Apple" || brand.Name == "Nike" || brand.Name == "Adidas")
                    .ToDictionaryAsync(brand => brand.Name, brand => brand.Id);

                if (categories.Count != 3 || brands.Count != 3)
                {
                    logger.LogWarning("Product seed data was skipped because the required categories or brands are unavailable.");
                    return;
                }

                await context.Products.AddRangeAsync(
                    new Product
                    {
                        Name = "iPhone 16",
                        Description = "A powerful Apple smartphone with an advanced camera system.",
                        BasePrice = 799.00m,
                        CategoryId = categories["Electronics"],
                        BrandId = brands["Apple"],
                        MainImageUrl = "https://images.unsplash.com/photo-1592286927505-1def25115558",
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "MacBook Air",
                        Description = "Lightweight laptop for work, study, and creativity.",
                        BasePrice = 1099.00m,
                        CategoryId = categories["Electronics"],
                        BrandId = brands["Apple"],
                        MainImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8",
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Nike Air Max",
                        Description = "Comfortable everyday sneakers with responsive cushioning.",
                        BasePrice = 149.99m,
                        CategoryId = categories["Shoes"],
                        BrandId = brands["Nike"],
                        MainImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff",
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Nike Dri-FIT T-Shirt",
                        Description = "Breathable performance shirt for training and casual wear.",
                        BasePrice = 35.00m,
                        CategoryId = categories["Clothes"],
                        BrandId = brands["Nike"],
                        MainImageUrl = "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab",
                        IsActive = true
                    },
                    new Product
                    {
                        Name = "Adidas Ultraboost",
                        Description = "Running shoes designed for energy return and comfort.",
                        BasePrice = 180.00m,
                        CategoryId = categories["Shoes"],
                        BrandId = brands["Adidas"],
                        MainImageUrl = "https://images.unsplash.com/photo-1552346154-21d32810aba3",
                        IsActive = true
                    });

                if (!await context.DeliveryMethods.AnyAsync())
                {
                    await context.DeliveryMethods.AddRangeAsync(
                        new DeliveryMethod
                        {
                            ShortName = "DHL",
                            DeliveryTime = "1-2 Days",
                            Description = "Fastest delivery time",
                            Price = 10m
                        },
                        new DeliveryMethod
                        {
                            ShortName = "Aramex",
                            DeliveryTime = "2-5 Days",
                            Description = "Get it within 5 days",
                            Price = 5m
                        },
                        new DeliveryMethod
                        {
                            ShortName = "FedEx",
                            DeliveryTime = "5-10 Days",
                            Description = "Slower but cheap",
                            Price = 2m
                        },
                        new DeliveryMethod
                        {
                            ShortName = "Free",
                            DeliveryTime = "1-2 Weeks",
                            Description = "Free shipping",
                            Price = 0m
                        }
                    );

                    await context.SaveChangesAsync();
                }

                await context.SaveChangesAsync();
            }
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while seeding the product catalog.");
        }
    }
}
