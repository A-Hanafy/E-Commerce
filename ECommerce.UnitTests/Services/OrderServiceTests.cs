using ECommerce.Core.Entities;
using ECommerce.Core.Entities.OrderAggregate;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Specifications;
using ECommerce.Infrastructure.Services;
using Moq;

namespace ECommerce.UnitTests.Services;

/// <summary>
/// Unit tests for <see cref="OrderService.CreateOrderAsync"/> and the read-side methods,
/// covering the core order-processing logic:
///   - basket validation (null / empty)
///   - stock validation (insufficient stock throws)
///   - price resolution (variant price with fallback to basket price)
///   - stock reservation (decrement + repository update)
///   - repurchase flow (delete existing intent-bound order + refresh payment intent)
///   - persistence guardrails (save failure returns null)
/// All external dependencies are mocked with Moq, so no database or Stripe/network
/// access ever happens — every test runs in complete isolation.
/// Each test follows the Arrange-Act-Assert (AAA) structure.
/// </summary>
public class OrderServiceTests
{
    private readonly Mock<IBasketRepository> _basketRepoMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IPaymentService> _paymentServiceMock = new();

    private readonly Mock<IGenericRepository<ProductVariant>> _variantRepoMock = new();
    private readonly Mock<IGenericRepository<DeliveryMethod>> _deliveryMethodRepoMock = new();
    private readonly Mock<IGenericRepository<Order>> _orderRepoMock = new();

    private const string BuyerEmail = "buyer@test.com";
    private const string BasketId = "basket-1";

    private readonly CustomerBasket _basket;
    private readonly DeliveryMethod _deliveryMethod;
    private readonly Address _shippingAddress;

    public OrderServiceTests()
    {
        _basket = new CustomerBasket(BasketId)
        {
            PaymentIntentId = "pi_test_123"
        };

        _deliveryMethod = new DeliveryMethod
        {
            Id = 1,
            ShortName = "Express",
            DeliveryTime = "1-2 Days",
            Price = 5m
        };

        _shippingAddress = new Address("John", "Doe", "1 Main St", "Springfield", "IL", "62701");

        // Static (closed-generic) wiring of IUnitOfWork.Repository<T>() — Moq requires
        // each closed generic overload to be stubbed explicitly.
        _ = _unitOfWorkMock
            .Setup(u => u.Repository<ProductVariant>())
            .Returns(_variantRepoMock.Object);
        _ = _unitOfWorkMock
            .Setup(u => u.Repository<DeliveryMethod>())
            .Returns(_deliveryMethodRepoMock.Object);
        _ = _unitOfWorkMock
            .Setup(u => u.Repository<Order>())
            .Returns(_orderRepoMock.Object);
    }

    private OrderService CreateSut() =>
        new(_basketRepoMock.Object, _unitOfWorkMock.Object, _paymentServiceMock.Object);

    /// <summary>
    /// Happy path: a valid basket with one in-stock variant produces an order with the
    /// correct buyer, address, payment intent, delivery method and subtotal.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WithValidBasket_ReturnsOrderWithCorrectDetails()
    {
        // Arrange
        const decimal unitPrice = 100m;
        const int quantity = 2;

        var item = new BasketItem
        {
            Id = 10,
            ProductName = "Air Max 90",
            Price = unitPrice,
            Quantity = quantity,
            PictureUrl = "images/airmax.png"
        };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = unitPrice, StockQuantity = 10 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(BuyerEmail, order.BuyerEmail);
        Assert.Equal("pi_test_123", order.PaymentIntentId);
        Assert.Equal(_deliveryMethod, order.DeliveryMethod);
        Assert.Equal(_shippingAddress, order.ShipToAddress);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(unitPrice * quantity, order.Subtotal);
        Assert.Single(order.Items);

        var orderItem = Assert.IsType<OrderItem>(order.Items.Single());
        Assert.Equal(unitPrice, orderItem.Price);
        Assert.Equal(quantity, orderItem.Quantity);
        Assert.Equal("Air Max 90", orderItem.ItemOrdered.ProductName);
    }

    /// <summary>
    /// Guard clause: an unknown basket id returns null instead of throwing.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenBasketDoesNotExist_ReturnsNull()
    {
        // Arrange
        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync((CustomerBasket?)null);
        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.Null(order);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    /// <summary>
    /// Guard clause: a basket with zero items returns null.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenBasketHasNoItems_ReturnsNull()
    {
        // Arrange
        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.Null(order);

        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    /// <summary>
    /// Stock validation: requesting more than available throws, and nothing is
    /// persisted or updated as a consequence.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenVariantHasInsufficientStock_ThrowsAndDoesNotPersist()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 5 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 3 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);

        var sut = CreateSut();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress));

        Assert.Contains("out of stock", ex.Message);

        _variantRepoMock.Verify(r => r.Update(It.IsAny<ProductVariant>()), Times.Never);
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Never);
    }

    /// <summary>
    /// Resilient ordering: a variant that no longer exists is silently skipped
    /// (continue branch) instead of failing the entire order.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenVariantNotFound_SkipsLineItem()
    {
        // Arrange
        var available = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 1 };
        var discontinued = new BasketItem { Id = 11, ProductName = "Retired Shoe", Price = 50m, Quantity = 1 };
        _basket.Items.Add(available);
        _basket.Items.Add(discontinued);

        var variant = new ProductVariant { Id = 10, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 10 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(variant);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(11)).ReturnsAsync((ProductVariant?)null);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.NotNull(order);
        var orderItem = Assert.Single(order.Items);
        Assert.Equal("Air Max 90", orderItem.ItemOrdered.ProductName);

        _variantRepoMock.Verify(r => r.Update(It.Is<ProductVariant>(v => v.StockQuantity == 9)), Times.Once);
    }

    /// <summary>
    /// Price fallback: an unpriced variant (null price) falls back to the catalog
    /// price carried on the basket item.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenVariantPriceIsNull_UsesBasketItemPrice()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 39.99m, Quantity = 3 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = null, StockQuantity = 10 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(39.99m * 3, order.Subtotal);

        var orderItem = Assert.Single(order.Items);
        Assert.Equal(39.99m, orderItem.Price);
    }

    /// <summary>
    /// Stock reservation: each fulfilled line item decrements StockQuantity and is
    /// persisted through the repository update.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WithSufficientStock_DecrementsVariantStock()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 4 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 10 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        _ = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.Equal(6, variant.StockQuantity);

        _variantRepoMock.Verify(r => r.Update(It.Is<ProductVariant>(v => v.Id == variant.Id && v.StockQuantity == 6)), Times.Once);
    }

    /// <summary>
    /// Persistence guardrail: if the save is rolled back (0 affected rows), the
    /// service returns null so the caller doesn't report a phantom order id.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenSaveFails_ReturnsNull()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 1 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 5 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(0);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.Null(order);

        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
    }

    /// <summary>
    /// Repurchase of an unfinished payment-intent order: the stale order is deleted
    /// and the payment intent is refreshed before the new order is created.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WhenOrderAlreadyExistsForPaymentIntent_DeletesStaleOrderAndRefreshesIntent()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 1 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 5 };
        var staleOrder = new Order(BuyerEmail, _shippingAddress, _deliveryMethod,
            new List<OrderItem>(), subtotal: 0m, paymentIntentId: _basket.PaymentIntentId);

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync(staleOrder);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        Assert.NotNull(order);
        Assert.NotSame(staleOrder, order);

        _orderRepoMock.Verify(r => r.Delete(staleOrder), Times.Once);
        _paymentServiceMock.Verify(p => p.CreateOrUpdatePaymentIntent(BasketId), Times.Once);

        _orderRepoMock.Verify(r => r.AddAsync(It.Is<Order>(o => o.PaymentIntentId == staleOrder.PaymentIntentId)), Times.Once);
        _unitOfWorkMock.Verify(u => u.CompleteAsync(), Times.Once);
    }

    /// <summary>
    /// The new order is enriched with the delivery method so GetTotal() includes shipping.
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_WithDeliveryMethod_ComputesTotalIncludingShipping()
    {
        // Arrange
        var item = new BasketItem { Id = 10, ProductName = "Air Max 90", Price = 100m, Quantity = 2 };
        _basket.Items.Add(item);

        var variant = new ProductVariant { Id = item.Id, SKU = "AE-90-RED-42", Price = 100m, StockQuantity = 10 };

        _ = _basketRepoMock.Setup(r => r.GetBasketAsync(BasketId)).ReturnsAsync(_basket);
        _ = _variantRepoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(variant);
        _ = _deliveryMethodRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_deliveryMethod);
        _ = _orderRepoMock.Setup(r => r.GetEntityWithSpecAsync(It.IsAny<ISpecification<Order>>())).ReturnsAsync((Order?)null);
        _ = _unitOfWorkMock.Setup(u => u.CompleteAsync()).ReturnsAsync(1);

        var sut = CreateSut();

        // Act
        var order = await sut.CreateOrderAsync(BuyerEmail, deliveryMethodId: 1, basketId: BasketId, shippingAddress: _shippingAddress);

        // Assert
        var storedOrder = _orderRepoMock.Invocations
            .Where(i => i.Method.Name == nameof(IGenericRepository<Order>.AddAsync))
            .Select(i => i.Arguments[0] as Order)
            .Single();

        Assert.NotNull(storedOrder);
        Assert.Equal(200m, storedOrder.Subtotal);
        Assert.Equal(205m, storedOrder.GetTotal());
        Assert.Equal(order, storedOrder);
    }

    // ---------------------------------------------------------------------
    // Read-side methods
    // ---------------------------------------------------------------------

    [Fact]
    public async Task GetOrdersForUserAsync_DelegatesToRepositoryWithSpec()
    {
        // Arrange
        var orders = new List<Order> { new(BuyerEmail, _shippingAddress, _deliveryMethod, new List<OrderItem>(), 50m, null) };
        IReadOnlyList<Order> expected = orders;

        _ = _orderRepoMock
            .Setup(r => r.GetAllWithSpecAsync(It.IsAny<OrdersWithItemsAndOrderingSpecification>()))
            .ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var result = await sut.GetOrdersForUserAsync(BuyerEmail);

        // Assert
        Assert.Same(expected, result);

        _orderRepoMock.Verify(r => r.GetAllWithSpecAsync(It.IsAny<OrdersWithItemsAndOrderingSpecification>()), Times.Once);
    }

    [Fact]
    public async Task GetOrderByIdAsync_DelegatesToRepositoryWithSpec()
    {
        // Arrange
        var order = new Order(BuyerEmail, _shippingAddress, _deliveryMethod, new List<OrderItem>(), 50m, null);

        _ = _orderRepoMock
            .Setup(r => r.GetEntityWithSpecAsync(It.IsAny<OrdersWithItemsAndOrderingSpecification>()))
            .ReturnsAsync(order);

        var sut = CreateSut();

        // Act
        var result = await sut.GetOrderByIdAsync(id: 5, buyerEmail: BuyerEmail);

        // Assert
        Assert.Same(order, result);
    }

    [Fact]
    public async Task GetDeliveryMethodsAsync_ReturnsAllDeliveryMethods()
    {
        // Arrange
        IReadOnlyList<DeliveryMethod> expected = new List<DeliveryMethod> { _deliveryMethod };

        _ = _deliveryMethodRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expected);

        var sut = CreateSut();

        // Act
        var result = await sut.GetDeliveryMethodsAsync();

        // Assert
        Assert.Same(expected, result);
    }
}
