using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Orden
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("ref"), MaxLength(32)]
        public string? Ref { get; set; } = string.Empty;
        [JsonProperty("coddes")]
        public int? Coddes { get; set; } = 0;
        [JsonProperty("codprd")]
        public int? Codprd { get; set; } = 0;
        [JsonProperty("vol")]
        public float? Vol { get; set; } = float.NaN;
        [JsonProperty("fchcar")]
        public DateTime? Fchcar { get; set; } = DateTime.MinValue;
        [JsonProperty("codest")]
        public int? Codest { get; set; }
        [JsonProperty("coduni")]
        public int? Coduni { get; set; } = 0;
        [JsonProperty("codchf")]
        public int? Codchf { get; set; } = 0;
        [JsonProperty("bolguiid"),MaxLength(256)]
        public string? Bulguiid { get; set; } = string.Empty;
        [JsonProperty("liniteid")]
        public int? Liniteid { get; set; }
        [JsonProperty("codprd2")]
        public int? Codprd2 { get; set; } = 0;
        [JsonProperty("dendes"), MaxLength(256)]
        public string? Dendes { get; set; } = string.Empty;
        [JsonProperty("vol2")]
        public float? Vol2 { get; set; } = float.NaN;
        [JsonProperty("batchId")]
        public int? BatchId { get; set; }
        [JsonProperty("CompartmentId")]
        public int? CompartmentId { get; set; }
        [JsonProperty("SealNumber"), MaxLength(128)]
        public string? SealNumber { get; set; } = string.Empty;
    }
}
