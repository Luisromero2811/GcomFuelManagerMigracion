using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Actividad
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("fch")]
		public DateTime? fch { get; set; } = DateTime.MinValue;

		[JsonProperty("codusu")]
		public int? codusu { get; set; } = 0;

		[JsonProperty("act"), MaxLength(256)]
		public string? act { get; set; } = string.Empty;

		[JsonProperty("fchfin")]
		public DateTime? fchfin { get; set; } = DateTime.MinValue;

		[JsonProperty("exit")]
		public bool? exit { get; set; } = false;
	}
}

