using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class ZonaCliente
    {
        [JsonProperty("zonaCod")] public int zonaCod { get; set; }
        [JsonProperty("cteCod")] public int cteCod { get; set; }

        [NotMapped] public Cliente Cliente { get; set; } = null!;
        [NotMapped] public Zona Zona { get; set; } = null!;
    }
}
