using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
	public class OrdenesDTO
	{
		public int? Referencia { get; set; }
        public DateTime? FechaPrograma { get; set; } = DateTime.MinValue;
        public int? EstatusOrden { get; set; }
		public DateTime? FechaCarga { get; set; } = DateTime.MinValue;
        public string Bol { get; set; } = string.Empty;
        public string DeliveryRack { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;

    }
}

