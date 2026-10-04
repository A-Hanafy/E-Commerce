using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs.OrderDtos
{
    public class OrderDto
    {
        [Required]
        public string BasketId { get; set; } = string.Empty;

        [Required]
        public int DeliveryMethodId { get; set; }

        [Required]
        public AddressDto ShipToAddress { get; set; } = null!;
    }
}