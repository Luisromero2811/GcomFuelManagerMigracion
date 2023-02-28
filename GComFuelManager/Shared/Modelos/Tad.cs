using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	//5
	public class Tad
	{
		[JsonProperty("cod"), Key]
		public Int16 cod { get; set; }

		[JsonProperty("den"), MaxLength(128)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("nro")]
		public Int16? nro { get; set; } = 0;

		[JsonProperty("activo")]
		public bool? activo { get; set; } = true; 
	}
}

