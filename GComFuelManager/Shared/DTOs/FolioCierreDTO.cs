using System;
using GComFuelManager.Shared.Modelos;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
	public class FolioCierreDTO
	{
		public string? Folio { get; set; } = null!;
		public Cliente? cliente { get; set; } = null!;
		public Destino? destino { get; set; } = null!;
		public Producto? Producto { get; set; } = null!;
		public DateTime? FchCierre { get; set; } = null!;
		public Grupo? Grupo { get; set; } = null!;
 	}
}

