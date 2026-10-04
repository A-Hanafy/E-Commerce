using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.Core.Entities;
using ECommerce.Core.Entities.Identity;

namespace ECommerce.API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<Address, AddressDto>().ReverseMap();

            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.ProductBrand, o => o.MapFrom(s => s.Brand.Name))
                .ForMember(d => d.ProductType, o => o.MapFrom(s => s.Category.Name))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.MainImageUrl))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.BasePrice));

            CreateMap<CustomerBasketDto, CustomerBasket>().ReverseMap();
            CreateMap<BasketItemDto, BasketItem>().ReverseMap();

            CreateMap<AddressDto, ECommerce.Core.Entities.OrderAggregate.Address>().ReverseMap();
        }
    }

}