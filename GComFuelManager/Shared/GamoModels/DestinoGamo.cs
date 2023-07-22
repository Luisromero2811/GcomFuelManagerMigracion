using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.GamoModels
{
    public class DestinoGamo
    {
        [JsonProperty("idDestino")]
        public string IdDestino { get; set; } = string.Empty;
        [JsonProperty("nombre")]
        public string Nombre { get; set; } = string.Empty;
        [JsonProperty("baja")]
        public bool Baja { get; set; } = true;
        [JsonProperty("idDestinoTuxpan")]
        public long IdDestinoTuxpan { get;set; } = 0;
        [JsonProperty("razonSocial")]
        public string RazonSocial { get; set; } = string.Empty;
        [JsonProperty("municipio")]
        public string Municipio { get; set; } = string.Empty;
        [JsonProperty("Ciudad")]
        public string Ciudad { get; set; } = string.Empty;
        [JsonProperty("Estado")]
        public string Estado { get; set; } = string.Empty;
    }

    public class DestinoGamoList
    {
        [JsonProperty("Destinos")]
        public List<DestinoGamo> Destinos { get; set;} = new List<DestinoGamo>();
    }
}
