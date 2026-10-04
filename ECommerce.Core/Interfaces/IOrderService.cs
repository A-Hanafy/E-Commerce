using ECommerce.Core.Entities.OrderAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Order?> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress);
        Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail);
        Task<Order?> GetOrderByIdAsync(int id, string buyerEmail);
        Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync();
    }
}
