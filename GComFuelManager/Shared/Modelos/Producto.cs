using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
	public class Producto
	{
        //5
        [JsonProperty("cod"), Key]
		public byte Cod { get; set; }

		[JsonProperty("den"), MaxLength(50)]
		public string? Den { get; set; } = string.Empty;

		[JsonProperty("codsyn"), MaxLength(10)]
		public string? Codsyn { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? Activo { get; set; } = true;

		public List<OrdenEmbarque> ordenEmbarque { get; set; } = new List<OrdenEmbarque>();
	}
}

