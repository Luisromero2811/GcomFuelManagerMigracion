using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Grupo
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(30)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("eje"), MaxLength(50)]
		public string? eje { get; set; } = string.Empty;

		[JsonProperty("fch")]
		public DateTime? fch { get; set; } = DateTime.MinValue;

		[JsonProperty("class")]
		public int? clase { get; set; } = 0;

		[JsonProperty("tipven"), MaxLength(16)]
		public string? tipven { get; set; } = string.Empty;
	}
}

