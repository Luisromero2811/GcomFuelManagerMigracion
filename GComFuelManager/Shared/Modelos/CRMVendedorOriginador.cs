using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMVendedorOriginador
    {
        public int VendedorId { get; set; }
        public int OriginadorId { get; set; }

        public CRMVendedor? Vendedor { get; set; } = null!;
        public CRMOriginador? Originador { get; set; } = null!;
    }
}
