using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Vendedor_Originador
    {
        public int VendedorId { get; set; } = 0;
        public int OriginadorId { get; set; } = 0;
        public Vendedor? Vendedor { get; set; } = null!;
        public Originador? Originador { get; set; } = null!;
        [NotMapped] public bool Borrar { get; set; } = false;
        public string Obtener_Nombre_Vnededor
        {
            get
            {
                if (Vendedor is not null)
                    if (!string.IsNullOrEmpty(Vendedor.Nombre))
                        return Vendedor.Nombre;
                return string.Empty;
            }
        }

        public string Obtener_Nombre_Originador
        {
            get
            {
                if (Originador is not null)
                    if (!string.IsNullOrEmpty(Originador.Nombre))
                        return Originador.Nombre;
                return string.Empty;
            }
        }
    }
}
