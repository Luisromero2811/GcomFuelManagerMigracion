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
    public class ZonaCliente
    {
        [Key, JsonProperty("cod")] public int Cod { get; set; }
        [JsonProperty("zonaCod")] public int? ZonaCod { get; set; }
        [JsonProperty("cteCod")] public int? CteCod { get; set; }
        [JsonProperty("desCod")] public int? DesCod { get; set; }
        [JsonProperty("activo")] public bool? Activo { get; set; } = true;

        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public Zona? Zona { get; set; } = null!;
        [NotMapped] public Destino? Destino { get; set; } = null!;
    }
}
