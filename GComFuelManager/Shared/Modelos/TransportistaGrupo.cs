using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class TransportistaGrupo
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("codtra")]
		public int codtra { get; set; } = 0;

		[JsonProperty("codgru")]
		public int codgru { get; set; } = 0;

		[JsonProperty("fch")]
		public DateTime fch { get; set; } = DateTime.MinValue;

	}
}

