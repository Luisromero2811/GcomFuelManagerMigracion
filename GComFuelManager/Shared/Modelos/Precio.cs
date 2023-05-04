using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Precio
    {
        [Key, JsonProperty("cod")] public byte Cod { get; set; }
        [JsonProperty("codZona")] public int codZona { get; set; }
        [JsonProperty("codCte")] public int codCte { get; set; }
        [JsonProperty("codPrd")] public byte codPrd { get; set; }
        [JsonProperty("pre")] public float pre { get; set; }
        [JsonProperty("fchActualizacion")] public DateTime FchActualizacion { get; set; }
    }
}
