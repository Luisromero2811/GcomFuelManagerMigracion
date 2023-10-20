using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs
{
    public class CodDenDTO
    {
        public int Cod { get; set; } = 0;
        public string Den { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public Cliente? cliente { get; set; }
        public Destino? destino { get; set; }
    }
}
