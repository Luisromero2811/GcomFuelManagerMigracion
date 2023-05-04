using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
	public class OrdenesDTO
	{
        public string? Referencia { get; set; } = string.Empty;
        public string? FechaPrograma { get; set; } = string.Empty;
        public string EstatusOrden { get; set; } = string.Empty;
		public string? FechaCarga { get; set; } = string.Empty;
        public int? Bol { get; set; } = 0;
        public string DeliveryRack { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public double? VolNat { get; set; } = 0;
        public double? VolCar { get; set; } = 0;
        public string Transportista { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
    }
}

