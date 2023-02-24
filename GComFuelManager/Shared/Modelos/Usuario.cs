using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Usuario
    {
        [JsonProperty("cod")]
        public int Cod { get; set; } = 0;
        [JsonProperty("den")]
        public string Den { get; set; } = string.Empty;
        [JsonProperty("usu")]
        public string Usu { get; set; } = string.Empty;
        [JsonProperty("cve")]
        public string Cve { get; set; } = string.Empty;
        [JsonProperty("fch")]
        public DateTime Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("tip")]
        public int Tip { get; set; } = 0;
        [JsonProperty("est")]
        public int Est { get; set; } = 0;
        [JsonProperty("privilegio")]
        public string Privilegio { get; set; } = string.Empty;
        [JsonProperty("activo")]
        public int Activo { get; set; } = 0;
    }
}
