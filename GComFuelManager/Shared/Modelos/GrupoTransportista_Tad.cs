using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
	public class GrupoTransportista_Tad
	{
		public int Id_GrupoTransportista { get; set; }
		public short Id_Terminal { get; set; }

		[NotMapped] public GrupoTransportista? GrupoTransportista { get; set; } = null!;
		[NotMapped] public Tad? Terminal { get; set; } = null!;
	}
}

