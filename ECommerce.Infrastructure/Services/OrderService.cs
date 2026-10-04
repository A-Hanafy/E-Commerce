using ECommerce.Core.Entities;
using ECommerce.Core.Entities.OrderAggregate;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Specifications;


namespace ECommerce.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IBasketRepository _basketRepo;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IBasketRepository basketRepo, IUnitOfWork unitOfWork)
        {
            _basketRepo = basketRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<Order?> CreateOrderAsync(string buyerEmail, int deliveryMethodId, string basketId, Address shippingAddress)
        {
            var basket = await _basketRepo.GetBasketAsync(basketId);
            if (basket == null || !basket.Items.Any()) return null;

            var items = new List<OrderItem>();

            foreach (var item in basket.Items)
            {
                var variant = await _unitOfWork.Repository<ProductVariant>().GetByIdAsync(item.Id);
                if (variant == null) continue;

                if (variant.StockQuantity < item.Quantity)
                {
                    throw new Exception($"Product variant '{item.ProductName}' is out of stock.");
                }

                variant.StockQuantity -= item.Quantity;
                _unitOfWork.Repository<ProductVariant>().Update(variant);

                var itemOrdered = new ProductItemOrdered(variant.Id, item.ProductName, item.PictureUrl);
                var price = variant.Price > 0 ? variant.Price : item.Price;
                var orderItem = new OrderItem(itemOrdered, Convert.ToDecimal( price), item.Quantity);
                items.Add(orderItem);
            }

            var deliveryMethod = await _unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(deliveryMethodId);
            var subtotal = items.Sum(item => item.Price * item.Quantity);
            var order = new Order(buyerEmail, shippingAddress, deliveryMethod!, items, subtotal);

            _unitOfWork.Repository<Order>().AddAsync(order);

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0) return null;

            await _basketRepo.DeleteBasketAsync(basketId);

            return order;
        }

        public async Task<IReadOnlyList<Order>> GetOrdersForUserAsync(string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(buyerEmail);
            return await _unitOfWork.Repository<Order>().GetAllWithSpecAsync(spec);
        }

        public async Task<Order?> GetOrderByIdAsync(int id, string buyerEmail)
        {
            var spec = new OrdersWithItemsAndOrderingSpecification(id, buyerEmail);
            return await _unitOfWork.Repository<Order>().GetEntityWithSpecAsync(spec);
        }

        public async Task<IReadOnlyList<DeliveryMethod>> GetDeliveryMethodsAsync()
        {
            return await _unitOfWork.Repository<DeliveryMethod>().GetAllAsync();
        }
    }
}