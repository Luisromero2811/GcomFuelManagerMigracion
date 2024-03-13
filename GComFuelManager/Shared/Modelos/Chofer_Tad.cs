using System;
using System.ComponentModel.DataAnnotations.Schema;
namespace GComFuelManager.Shared.Modelos
{
	public class Chofer_Tad
	{
		public int Id_Chofer { get; set; }
		public short Id_Terminal { get; set; }

		[NotMapped] public Chofer? Chofer { get; set; } = null!;
		[NotMapped] public Tad? Terminal { get; set; } = null!;

	}
}

