using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.API.Helpers;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Specifications;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController(IUnitOfWork unitOfWork,IMapper mapper) : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IMapper _mapper = mapper;

    [HttpGet]
    public async Task<ActionResult<Pagination<ProductToReturnDto>>> GetProducts([FromQuery] ProductSpecParams specParams)
    {
        var specification = new ProductsWithTypesAndBrandsSpecification(specParams);
        var products = await _unitOfWork.Repository<Product>().GetAllWithSpecAsync(specification);
        var countSpecification = new ProductsWithFiltersForCountSpecification(specParams);
        var totalItems = await _unitOfWork.Repository<Product>().CountAsync(countSpecification);


        var productDtos = _mapper.Map<IReadOnlyList<Product>, IReadOnlyList<ProductToReturnDto>>(products);

        return Ok(new Pagination<ProductToReturnDto>(
            specParams.PageIndex,
            specParams.PageSize,
            totalItems,
            productDtos));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductToReturnDto>> GetProduct(int id)
    {
        var specification = new ProductsWithTypesAndBrandsSpecification(id);
        var product = await _unitOfWork.Repository<Product>().GetEntityWithSpecAsync(specification);
        if (product is null)
        {
            return this.ToNotFoundResult();
        }

        return Ok(_mapper.Map<Product, ProductToReturnDto>(product));
    }

    
}
