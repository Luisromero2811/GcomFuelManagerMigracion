using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Inventario
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("invini")]
        public float? Invini { get; set; } = float.NaN;
        [JsonProperty("codprd")]
        public int? Codprd { get; set; } = 0;
        [JsonProperty("com")]
        public float? Com { get; set; } = float.NaN;
        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("pro")]
        public string? Pro { get; set; } = string.Empty;
        [JsonProperty("fchcom")]
        public DateTime? Fchcom { get; set; } = DateTime.MinValue;
        [JsonProperty("ful")]
        public float? Ful { get; set; } = float.NaN;
    }
}
