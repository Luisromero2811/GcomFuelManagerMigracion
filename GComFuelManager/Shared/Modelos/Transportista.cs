using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.ComponentModel;

namespace GComFuelManager.Shared.Modelos
{
    public class Transportista
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; } = 0;

        [JsonProperty("den"), MaxLength(256)]
        public string? Den { get; set; } = string.Empty;

        [JsonProperty("carrId"), AllowNull, DefaultValue("")]
        public string? CarrId { get; set; } = string.Empty;

        [JsonProperty("busentid"), MaxLength(15)]
        public string? Busentid { get; set; } = string.Empty;

        [JsonProperty("activo")]
        public bool? Activo { get; set; } = true;

        [JsonProperty("simsa")]
        public bool? Simsa { get; set; } = true;

        [JsonProperty("gru")]
        public string? Gru { get; set; } = string.Empty;
        public string? RFC { get; set; } = string.Empty;
    }
}

