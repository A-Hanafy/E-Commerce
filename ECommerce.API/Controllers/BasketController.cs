using AutoMapper;
using ECommerce.API.DTOs;
using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BasketController(IBasketRepository basketRepository,IMapper mapper) : ControllerBase
    {
        private readonly IBasketRepository _basketRepository = basketRepository;
        private readonly IMapper _mapper = mapper;

        [HttpGet]
        public async Task<IActionResult> GetBasketById(string id)
        {
            var basket = await _basketRepository.GetBasketAsync(id);
            return Ok(basket ?? new CustomerBasket(id));

        }

        [HttpPost]
        public async Task<ActionResult<CustomerBasket>> UpdateBasket(CustomerBasketDto basketDto)
        {
            var basket =_mapper.Map<CustomerBasketDto,CustomerBasket>(basketDto);
            var updatedBasket = await _basketRepository.UpdateBasketAsync(basket);
            return Ok(_mapper.Map<CustomerBasket,CustomerBasketDto>(updatedBasket!));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteBasketAsync(string id)
        {
            await _basketRepository.DeleteBasketAsync(id);
            return Ok();
        }
    }
}
