using Microsoft.AspNetCore.Identity;

namespace GComFuelManager.Server.Identity
{
    public class IdentityRol : IdentityRole
    {
        public bool Show { get; set; } = true;
    }
}
