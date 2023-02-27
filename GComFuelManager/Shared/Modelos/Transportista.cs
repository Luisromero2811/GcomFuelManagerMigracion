using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Transportista
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(256)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("carrId"), MaxLength(15)]
		public string? carrId { get; set; } = string.Empty;

		[JsonProperty("busentid"), MaxLength(15)]
		public string? busentid { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? activo { get; set; } = true;

		[JsonProperty("simsa")]
		public bool? simsa { get; set; } = true;

		[JsonProperty("gru"), MaxLength(32)]
		public string? gru { get; set; } = string.Empty;

	}
}

