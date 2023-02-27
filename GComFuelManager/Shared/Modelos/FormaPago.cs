using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
namespace GComFuelManager.Shared.Modelos
{
	public class FormaPago
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("den"), MaxLength(20)]
		public string? den { get; set; } = string.Empty;
	}
}

