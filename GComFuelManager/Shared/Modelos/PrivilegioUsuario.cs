using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class PrivilegioUsuario
	{
		[JsonProperty("cod"), Key]
		public int cod { get; set; }

		[JsonProperty("codpri")]
		public int? codpri { get; set; } = 0;

		[JsonProperty("codusu")]
		public int? codusu { get; set; } = 0;

		[JsonProperty("fch")]
		public DateTime? fch { get; set; } = DateTime.MinValue;

		[JsonProperty("activo")]
		public bool? activo { get; set; } = true;

	}
}

