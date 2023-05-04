using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Zona
    {
        [JsonProperty("cod")] public int Cod { get; set; }
        [JsonProperty("nombre")] public string Nombre { get; set; } = string.Empty;

        [NotMapped] public ZonaCliente ZonaCliente { get; set; } = null!;
    }
}
