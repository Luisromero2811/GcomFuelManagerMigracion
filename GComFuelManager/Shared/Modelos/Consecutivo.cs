using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Consecutivo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Numeracion { get; set; } = 0;
    }
}
