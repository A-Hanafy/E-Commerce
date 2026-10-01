using Microsoft.AspNetCore.Identity;

using System.Net;

namespace ECommerce.Core.Entities.Identity
{
    public class AppUser : IdentityUser
    {
        public string DisplayName { get; set; } = string.Empty;
        public Address? Address { get; set; }
    }
}