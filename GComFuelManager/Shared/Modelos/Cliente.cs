using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Cliente
    {
        [JsonProperty("cod")]
        public int Cod { get; set; }
        [JsonProperty("den"), MaxLength(128)]
        public string? Den { get; set; } = string.Empty;
        [JsonProperty("codusu")]
        public int? Codusu { get; set; } = 0;
        [JsonProperty("codforpag")]
        public string? Codforpag { get; set; } = string.Empty;
        [JsonProperty("tem"), MaxLength(50)]
        public string? Tem { get; set; } = string.Empty;
        [JsonProperty("codgru")]
        public string? Codgru { get; set; } = string.Empty;
        [JsonProperty("email"), MaxLength(30)]
        public string? Email { get; set; } = string.Empty;
        [JsonProperty("con"), MaxLength(50)]
        public string? Con { get; set; } = string.Empty;
        [JsonProperty("codtad")]
        public string? Cadtad { get; set; } = string.Empty;
        [JsonProperty("codsyn"), MaxLength(20)]
        public string? Codsyn { get; set; } = string.Empty;
        [JsonProperty("esenergas")]
        public bool? Esenergas { get; set; } = false;
        [JsonProperty("tipven"), MaxLength(16)]
        public string? Tipven { get; set; } = string.Empty;
    }
}
