using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
namespace GComFuelManager.Shared.Modelos
{
	public class Privilegio
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("pri"), MaxLength(256)]
		public string? pri { get; set; } = string.Empty;

		[JsonProperty("fch")]
		public DateTime? fch = DateTime.MinValue;

		[JsonProperty("act")]
		public bool? act { get; set; } = true;
	}
}

