using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Contacto
    {
        [Key]
        public int Cod { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int? CodCte { get; set; } = 0;
        public bool Estado { get; set; } = true;
        [NotMapped]
        public Cliente? Cliente { get; set; } = null!;
    }
}
