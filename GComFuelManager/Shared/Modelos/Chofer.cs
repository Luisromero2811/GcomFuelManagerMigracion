using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace GComFuelManager.Shared.Modelos
{
	public class Chofer
	{
		[JsonProperty("cod"), Key]
		public int Cod { get; set; }

		[JsonProperty("den"), MaxLength(128)]
		public string? Den { get; set; } = string.Empty;

		[JsonProperty("codtransport")]
		public int? Codtransport { get; set; } = 0;

		[JsonProperty("dricod"), MaxLength(6)]
		public string? Dricod { get; set; } = string.Empty;

		[JsonProperty("shortden"), MaxLength(128)]
		public string? Shortden { get; set; } = string.Empty;

		[JsonProperty("activo")]
		public bool? Activo { get; set; } = true;
		public string? RFC { get; set; } = string.Empty;

		[NotMapped] public Transportista? Transportista { get; set; } = null!;
		[NotMapped] public Tonel? Tonel { get; set; } = null!;
    }
}

