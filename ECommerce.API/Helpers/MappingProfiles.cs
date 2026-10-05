using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.API.DTOs.OrderDtos;
using ECommerce.Core.Entities;
using ECommerce.Core.Entities.Identity;
using ECommerce.Core.Entities.OrderAggregate;

namespace ECommerce.API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            CreateMap<ECommerce.Core.Entities.Identity.Address, AddressDto>().ReverseMap();
            CreateMap<Product, ProductToReturnDto>()
                .ForMember(d => d.ProductBrand, o => o.MapFrom(s => s.Brand.Name))
                .ForMember(d => d.ProductType, o => o.MapFrom(s => s.Category.Name))
                .ForMember(d => d.PictureUrl, o => o.MapFrom(s => s.MainImageUrl))
                .ForMember(d => d.Price, o => o.MapFrom(s => s.BasePrice));

            CreateMap<CustomerBasketDto, CustomerBasket>().ReverseMap();
            CreateMap<BasketItemDto, BasketItem>().ReverseMap();

            CreateMap<Order, OrderToReturnDto>()
                  .ForMember(d => d.DeliveryMethod, o => o.MapFrom(s => s.DeliveryMethod.ShortName))
                  .ForMember(d => d.ShippingPrice, o => o.MapFrom(s => s.DeliveryMethod.Price));

            CreateMap<OrderItem, OrderItemDto>();

            CreateMap<AddressDto, ECommerce.Core.Entities.OrderAggregate.Address>().ReverseMap();
        }
    }

}