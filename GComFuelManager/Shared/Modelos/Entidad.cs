using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
namespace GComFuelManager.Shared.Modelos
{
	public class Entidad
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(50)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("fch")]
		public DateTime? fch { get; set; } = DateTime.MinValue;
	}
}

