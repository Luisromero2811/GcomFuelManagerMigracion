using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class OrdenCompra
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(1000)]
		public string? den { get; set; } = string.Empty;
	}
}

