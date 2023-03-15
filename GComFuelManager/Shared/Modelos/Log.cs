using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
namespace GComFuelManager.Shared.Modelos
{
	public class Log
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("codusu")]
		public int? codusu { get; set; } = 0;

		[JsonProperty("fch")]
		public DateTime? fch { get; set; } = DateTime.MinValue;
	}
}

