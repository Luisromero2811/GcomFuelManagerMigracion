using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Actividad
	{
		[JsonProperty("cod"),Key]
		public int Cod { get; set; }

		[JsonProperty("fch")]
		public DateTime? Fch { get; set; } = DateTime.MinValue;

		[JsonProperty("codusu")]
		public int? Codusu { get; set; } = 0;

		[JsonProperty("act"), MaxLength(256)]
		public string? Act { get; set; } = string.Empty;

		[JsonProperty("fchfin")]
		public DateTime? Fchfin { get; set; } = DateTime.MinValue;

		[JsonProperty("exit")]
		public bool? Exit { get; set; } = false;
	}
}

