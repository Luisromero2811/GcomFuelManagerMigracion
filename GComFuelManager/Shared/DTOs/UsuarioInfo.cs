using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class UsuarioInfo
    {
        public string Id { get; set; } = string.Empty;
        public int UserCod { get; set; }
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
        public string Password { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

    }
}
