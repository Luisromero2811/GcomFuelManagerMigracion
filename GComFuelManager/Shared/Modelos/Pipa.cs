using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Pipa
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("cve"), MaxLength(10)]
		public string? cve { get; set; } = string.Empty;

		[JsonProperty("cap")]
		public int? cap { get; set; } = 0;

		[JsonProperty("placa"), MaxLength(10)]
		public string? placa { get; set; } = string.Empty;

		[JsonProperty("codest")]
		public int? codest { get; set; } = 0;

		[JsonProperty("codtransp")]
		public int? codtransp { get; set; } = 0;

		[JsonProperty("nroeco"), MaxLength(50)]
		public string? nroeco { get; set; } = string.Empty;
	}
}

