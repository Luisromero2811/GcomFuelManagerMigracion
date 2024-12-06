using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class InventarioExcelDTO
    {
        [DisplayName("Fecha de cierre")]
        public DateTime? FechaCierre { get; set; } = null;
        [DisplayName("Fecha de registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Today;
        public string Producto { get; set; } = string.Empty;
        public string Sitio { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        [DisplayName("Tipo de movimiento")]
        public string TipoMovimiento { get; set; } = string.Empty;
        [DisplayName("ID Documento")]
        public string Numero { get; set; } = string.Empty;
        [DisplayName("Fecha de movimiento")]
        public DateTime FechaMovimiento { get; set; } = DateTime.Today;
        [DisplayName("Hora de inicio de movimiento")]
        public DateTime FechaInicioMovimiento { get; set; } = DateTime.Now;
        [DisplayName("Hora de fin de movimiento")]
        public DateTime FechaFinMovimiento { get; set; } = DateTime.Now;
        [DisplayName("Tirilla inicial")]
        public double TirillaInicial { get; set; }
        [DisplayName("Tirilla final")]
        public double TirillaFinal { get; set; }
        public double Cantidad { get; set; }
        [DisplayName("Cantidad facturada")]
        public double CantidadFacturada { get; set; }
        [DisplayName("Sobrante / Faltante")]
        public double Diferencia { get; set; }
        [DisplayName("Unidad de medida")]
        public string UnidadMedida { get; set; } = string.Empty;
        public double Temperatura { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Transportista { get; set; } = string.Empty;
        public string Tonel { get; set; } = string.Empty;
        public string Chofer { get; set; } = string.Empty;
        public string TipoInventario { get; set; } = string.Empty;
        public string OrigenDestino { get; set; } = string.Empty;
    }
}
