using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
	public class FechasF
	{
		public DateTime DateInicio { get; set; } = DateTime.Today.Date;
		public DateTime DateFin { get; set; } = DateTime.Now;
		public int Estado { get; set; } = 1;
		public string TipVenta { get; set; } = string.Empty!;
	}
}

