using ECommerce.Core.Entities;
using ECommerce.Core.Entities.OrderAggregate;
using ECommerce.Core.Interfaces;
using ECommerce.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerce.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly StripeSettings _stripeSettings;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(
            IPaymentService paymentService,
            IOptions<StripeSettings> stripeSettings,
            ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _stripeSettings = stripeSettings.Value;
            _logger = logger;
        }

        //[Authorize]
        [HttpPost("{basketId}")]
        [EnableRateLimiting("paymentPolicy")]
        public async Task<ActionResult<CustomerBasket>> CreateOrUpdatePaymentIntent(string basketId)
        {
            var basket = await _paymentService.CreateOrUpdatePaymentIntent(basketId);

            if (basket == null) return BadRequest("Problem with your basket");

            return Ok(basket);
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeSettings.WebhookSecret
                );

                PaymentIntent intent;
                Order? order;

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        _logger.LogInformation("Payment Succeeded: {Id}", intent.Id);
                        order = await _paymentService.UpdateOrderPaymentSucceeded(intent.Id);
                        _logger.LogInformation("Order updated to payment received: {Id}", order?.Id);
                        break;

                    case "payment_intent.payment_failed":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        _logger.LogInformation("Payment Failed: {Id}", intent.Id);
                        order = await _paymentService.UpdateOrderPaymentFailed(intent.Id);
                        _logger.LogInformation("Order updated to payment failed: {Id}", order?.Id);
                        break;
                }

                return new EmptyResult();
            }
            catch (StripeException e)
            {
                _logger.LogError(e, "Stripe Webhook Error");
                return BadRequest();
            }
        }
    }
}