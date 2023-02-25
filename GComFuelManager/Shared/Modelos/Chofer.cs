using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Chofer
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(128)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("codtransport")]
		public int? codtransport { get; set; } = 0;

		[JsonProperty("dricod"), MaxLength(6)]
		public string? dricod { get; set; } = string.Empty;

		[JsonProperty("shortden"), MaxLength(128)]
		public string? shortden { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? activo { get; set; } = true;
	}
}

