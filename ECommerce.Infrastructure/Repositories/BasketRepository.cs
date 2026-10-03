using ECommerce.Core.Entities;
using ECommerce.Core.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace ECommerce.Infrastructure.Repositories
{
    public class BasketRepository(IDistributedCache cache) : IBasketRepository
    {
        private readonly IDistributedCache _cache = cache;


        public async Task<CustomerBasket?> GetBasketAsync(string basketId)
        {
            var data = await _cache.GetAsync(basketId);
            return data ==null ? null : JsonSerializer.Deserialize<CustomerBasket>(data);
        }

        public async Task<CustomerBasket?> UpdateBasketAsync(CustomerBasket basket)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30) // حفظ السلة لمدة 30 يوم
            };

            var serializedBasket = JsonSerializer.Serialize(basket);
            await _cache.SetStringAsync(basket.Id, serializedBasket, options);

            return await GetBasketAsync(basket.Id);
        }
        public async Task<bool> DeleteBasketAsync(string basketId)
        {
            await _cache.RemoveAsync(basketId);
            return true;

        }
    }
}
