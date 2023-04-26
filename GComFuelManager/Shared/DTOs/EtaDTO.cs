using System;
namespace GComFuelManager.Shared.DTOs
{
	public class EtaDTO
	{
        public string? Referencia { get; set; } = string.Empty;
        public string? FechaPrograma { get; set; } = string.Empty;
        public string EstatusOrden { get; set; } = string.Empty;
        public string? FechaCarga { get; set; } = string.Empty;
        public Int64? Bol { get; set; } = null!;
        public string Cliente { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public double? VolNat { get; set; } = 0;
        public double? VolCar { get; set; } = 0;
        public string Transportista { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;

        public DateTime? FechaDoc { get; set; } = DateTime.Now;
        public string Eta { get; set; } = string.Empty;
        public DateTime? FechaEst { get; set; } = DateTime.Now;
        public string Observaciones { get; set; } = string.Empty;
        public DateTime? FechaRealEta { get; set; } = DateTime.Now;
        public double? LitEnt { get; set; } = 0;

    }
}

