using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
	//5
	public class Tad
	{
		[JsonProperty("cod"), Key]
		public Int16 Cod { get; set; }

		[JsonProperty("den"), MaxLength(128)]
		public string? Den { get; set; } = string.Empty;

		[JsonProperty("nro")]
		public Int16? Nro { get; set; } = 0;

		[JsonProperty("activo")]
		public bool? Activo { get; set; } = true;

		public List<OrdenEmbarque> ordenEmbarque { get; set; } = new List<OrdenEmbarque>();
    }
}

