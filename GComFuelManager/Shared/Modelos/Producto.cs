using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
	public class Producto
	{
        //5
        [JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(50)]
		public string? den { get; set; } = string.Empty;

		[JsonProperty("codsyn"), MaxLength(10)]
		public string? codsyn { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? activo { get; set; } = true;
	}
}

