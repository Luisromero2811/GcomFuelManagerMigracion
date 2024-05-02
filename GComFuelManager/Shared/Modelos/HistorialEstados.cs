using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class HistorialEstados
    {
        public int Id { get; set; }
        public byte Id_Estado { get; set; }
        public int Id_Orden { get; set; }
        public string Id_Usuario { get; set; } = string.Empty;
        public DateTime Fecha_Actualizacion { get; set; } = DateTime.Now;

        [NotMapped] public OrdenEmbarque? OrdenEmbarque { get; set; } = null!;
        [NotMapped] public Estado? Estado { get; set; } = null!;
    }
}
