using System.ComponentModel.DataAnnotations;

namespace ECommerce.API.DTOs
{
    public class RegisterDto
    {
        [Required]
        public string DisplayName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+}{"":;'?/>.<,]).{6,}$",
                    ErrorMessage = "Password must have 1 Upper case, 1 Lower case, 1 number, 1 non alphanumeric and at least 6 characters")]
        public string Password { get; set; } = string.Empty;
    }
}
