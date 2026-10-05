using ECommerce.Core.Entities;
using ECommerce.Core.Entities.OrderAggregate;


namespace ECommerce.Core.Interfaces
{
    public interface IPaymentService
    {
        Task<CustomerBasket?> CreateOrUpdatePaymentIntent(string basketId);

        Task<Order?> UpdateOrderPaymentSucceeded(string paymentIntentId);

        Task<Order?> UpdateOrderPaymentFailed(string paymentIntentId);
    }
}
