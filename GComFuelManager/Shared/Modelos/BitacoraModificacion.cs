using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class BitacoraModificacion
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("codordemb")]
        public int? Codordemb { get; set; } = 0;
        [JsonProperty("fchmod")]
        public DateTime? Fchmod { get; set; } = DateTime.MinValue;
        [JsonProperty("fchcar")]
        public DateTime? Fchcar { get; set; } = DateTime.MinValue;
        [JsonProperty("coddes")]
        public int? Coddes { get; set; } = 0;
        [JsonProperty("codusu")]
        public int? Codusu { get; set; } = 0;
        [JsonProperty("codpip")]
        public int? Codpip { get; set; } = 0;
        [JsonProperty("codton")]
        public int? Codton { get; set; } = 0;
        [JsonProperty("codchf")]
        public int? Codchf { get; set; } = 0;
    }
}
