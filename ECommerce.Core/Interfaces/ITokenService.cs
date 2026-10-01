using ECommerce.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Core.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(AppUser user);
    }
}
