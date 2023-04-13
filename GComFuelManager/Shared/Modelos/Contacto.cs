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
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [JsonProperty("correo")]
        public string Correo { get; set; } = string.Empty;
        [JsonProperty("codCte")]
        public int CodCte { get; set; }
        [JsonProperty("estatus")]
        public bool Estado { get; set; } = true;
        [NotMapped]
        public Cliente? Cliente { get; set; } = null!;
    }
}
