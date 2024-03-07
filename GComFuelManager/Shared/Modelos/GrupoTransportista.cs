using System;
using System.ComponentModel.DataAnnotations;
using GComFuelManager.Shared.DTOs;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
	public class GrupoTransportista
	{
        [Key, JsonProperty("cod")] public int cod { get; set; }

        [JsonProperty("den"), MaxLength(255)]
        public string? den { get; set; } = string.Empty;
    }
}

