using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Destino
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("num")]
        public int? Num { get; set; } = 0;
        [JsonProperty("den"), MaxLength(128)]
        public string? Den { get; set; } = string.Empty;
        [JsonProperty("codcte")]
        public int? Codcte { get; set; } = 0;
        [JsonProperty("nroper"), MaxLength(30)]
        public string? Nroper { get; set; } = string.Empty;
        [JsonProperty("dir")]
        public string? Dir { get; set; } = string.Empty;
        [JsonProperty("codbdTan")]
        public int? CodbdTan { get; set; } = 0;
        [JsonProperty("descod"), MaxLength(10)]
        public string? DesCod { get; set; } = string.Empty;
        [JsonProperty("codsyn"), MaxLength(20)]
        public string? Codsyn { get; set; } = string.Empty;
        [JsonProperty("esenergas")]
        public bool? Esenergas { get; set; } = false;
        [JsonProperty("activo")]
        public bool? Activo { get; set; } = true;
        [JsonProperty("lat"), MaxLength(50)]
        public string? Lat { get; set; } = string.Empty;
        [JsonProperty("lon"), MaxLength(50)]
        public string? Lon { get; set; } = string.Empty;
        [JsonProperty("codciu")]
        public int? Codciu { get; set; } = 0;
        [JsonProperty("ciu"), MaxLength(50)]
        public string? Ciu { get; set; } = string.Empty;
        [JsonProperty("est"), MaxLength(50)]
        public string? Est { get; set; } = string.Empty;

        public List<OrdenEmbarque> OrdenEmbarque { get; set; } = null!;
    }
}
