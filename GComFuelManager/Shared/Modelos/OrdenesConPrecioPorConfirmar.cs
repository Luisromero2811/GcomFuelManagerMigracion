using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenesConPrecioPorConfirmar
    {
        public int Id { get; set; }
        public string Guiid { get; set; } = string.Empty;
        public int OrdenCierreId { get; set; }
    }
}
