using ECommerce.Core.Entities;

namespace ECommerce.Core.Specifications;

public sealed class ProductsWithTypesAndBrandsSpecification : BaseSpecification<Product>
{
    public ProductsWithTypesAndBrandsSpecification(ProductSpecParams productSpecParams)
        : base(product =>
            (!productSpecParams.BrandId.HasValue || product.BrandId == productSpecParams.BrandId.Value) &&
            (!productSpecParams.CategoryId.HasValue || product.CategoryId == productSpecParams.CategoryId.Value) &&
            (string.IsNullOrEmpty(productSpecParams.Search) ||
             product.Name.ToLower().Contains(productSpecParams.Search)))
    {
        AddProductIncludes();
        ApplySorting(productSpecParams.Sort);
        ApplyPaging(productSpecParams.PageSize * (productSpecParams.PageIndex - 1), productSpecParams.PageSize);
    }

    public ProductsWithTypesAndBrandsSpecification(int id)
        : base(product => product.Id == id)
    {
        AddProductIncludes();
    }

    private void AddProductIncludes()
    {
        AddInclude(product => product.Brand);
        AddInclude(product => product.Category);
    }

    private void ApplySorting(string? sort)
    {
        switch (sort)
        {
            case "priceAsc":
                AddOrderBy(product => product.BasePrice);
                break;
            case "priceDesc":
                AddOrderByDescending(product => product.BasePrice);
                break;
            default:
                AddOrderBy(product => product.Name);
                break;
        }
    }
}
