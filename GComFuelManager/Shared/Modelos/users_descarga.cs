using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
	[Table("users_descargas")]
	public class users_descarga
	{
		[JsonProperty("iduser")]
		public int iduser { get; set; }

		[JsonProperty("usuario"), MaxLength(50)]
		public string? usuario { get; set; } = string.Empty;

		[JsonProperty("pass"), MaxLength(50)]
		public string? pass { get; set; } = string.Empty;

		[JsonProperty("idDestino")]
		public int? idDestino { get; set; } = 0;

		[JsonProperty("estatus")]
		public bool? estatus { get; set; } = true;

		[JsonProperty("tipo")]
		public bool? tipo { get; set; } = true;

		[JsonProperty("fecha_creacion")]
		public DateTime? fecha_creacion { get; set; } = DateTime.MinValue;
	}
}

