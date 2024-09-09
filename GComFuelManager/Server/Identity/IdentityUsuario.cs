using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Server.Identity
{
    public class IdentityUsuario:IdentityUser
    {
        public int UserCod { get; set; }

        public bool Activo { get; set; } = true;

        [NotMapped]
        public Usuario Usuario { get; set; } = null!;
    }
}
