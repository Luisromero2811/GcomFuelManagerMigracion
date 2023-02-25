using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
namespace GComFuelManager.Shared.Modelos
{
	public class Version
	{
		[JsonProperty("cod")]
		public int cod { get; set; }

		[JsonProperty("ver"), MaxLength(50)]
		public string? ver { get; set; } = string.Empty;

		[JsonProperty("car"), MaxLength(512)]
		public string? car { get; set; } = string.Empty;

		[JsonProperty("act")]
		public bool? act { get; set; } = true;

		[JsonProperty("fchlib")]
		public DateTime? fchlib { get; set; } = DateTime.MinValue;
	}
}

