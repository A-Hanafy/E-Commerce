using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs
{
    public class CustomerBasketDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        public List<BasketItemDto> Items { get; set; } = new List<BasketItemDto>();
    }
}
