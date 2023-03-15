using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
namespace GComFuelManager.Shared.Modelos
{
    public class Usuario
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("den"),MaxLength(64)]
        public string? Den { get; set; } = string.Empty;
        [JsonProperty("usu"), MaxLength(10)]
        public string? Usu { get; set; } = string.Empty;
        [JsonProperty("cve"), MaxLength(10)]
        public string? Cve { get; set; } = string.Empty;
        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("tip")]
        public int? Tip { get; set; } = 0;
        [JsonProperty("est")]
        public int? Est { get; set; } = 0;
        [JsonProperty("privilegio"), MaxLength(256)]
        public string? Privilegio { get; set; } = string.Empty;
        [JsonProperty("activo")]
        public bool? Activo { get; set; } = true;

        [NotMapped] public Cliente? Cliente { get; set; }
    }
}
