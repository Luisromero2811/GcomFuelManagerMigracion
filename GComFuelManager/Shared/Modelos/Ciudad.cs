using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
namespace GComFuelManager.Shared.Modelos
{
	public class Ciudad
	{
		[JsonProperty("cod"), Key]
		public int Cod { get; set; }

		[JsonProperty("den"), MaxLength(128)]
		public string? Den { get; set; } = string.Empty;

		[JsonProperty("fch")]
		public DateTime? Fch { get; set; } = DateTime.MinValue;

		[JsonProperty("codent")]
		public int? Codent { get; set; } = 0;
	}
}

