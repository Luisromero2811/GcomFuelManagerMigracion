using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class AccionCorreo
    {
        [Key] public int? Cod { get; set; }
        public Int16? CodAccion { get; set; }
        public int CodContacto { get; set; }
        [NotMapped] public Accion? Accion { get; set; } = null!;
        [NotMapped] public Contacto? Contacto { get; set; } = null!;
    }
}
