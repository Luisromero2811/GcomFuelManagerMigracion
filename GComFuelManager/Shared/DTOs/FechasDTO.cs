using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
	public class FechasF
	{
		public DateTime DateInicio { get; set; } = DateTime.MinValue;
		public DateTime DateFin { get; set; } = DateTime.MinValue;
		public bool SinCargar { get; set; } = false;
		public bool Cargadas { get; set; } = false;
		public bool EnTrayecto { get; set; } = false;
	}
}

