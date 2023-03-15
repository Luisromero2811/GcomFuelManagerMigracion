using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GComFuelManager.Shared.DTOs
{
	public class HistorialDespachoDTO
	{
		public string? Referencia { get; set; } = string.Empty;
		public string? FechaPrograma { get; set; } = string.Empty;
		public string? EstatusOrden { get; set; } = string.Empty;
		public string? FechaCarga { get; set; } = string.Empty;
		public string? Bol { get; set; } = string.Empty;
		public string? DeliveryRack { get; set; } = string.Empty;
		public string? Cliente { get; set; } = string.Empty;
		public string? Destino { get; set; } = string.Empty;
		public string? Producto { get; set; } = string.Empty;
		public int? VolumenNat { get; set; }
		public int? VolumenCar { get; set; }
		public string? Transportista { get; set; } = string.Empty;
		public string? Unidad { get; set; } = string.Empty;
		public string? Operador { get; set; } = string.Empty;
	}
}

