using ECommerce.Core.Entities;

namespace ECommerce.Core.Specifications;

public sealed class ProductsWithFiltersForCountSpecification : BaseSpecification<Product>
{
    public ProductsWithFiltersForCountSpecification(ProductSpecParams productSpecParams)
        : base(product =>
            (!productSpecParams.BrandId.HasValue || product.BrandId == productSpecParams.BrandId.Value) &&
            (!productSpecParams.CategoryId.HasValue || product.CategoryId == productSpecParams.CategoryId.Value) &&
            (string.IsNullOrEmpty(productSpecParams.Search) ||
             product.Name.ToLower().Contains(productSpecParams.Search)))
    {
    }
}
