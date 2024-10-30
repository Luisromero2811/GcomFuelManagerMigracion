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
        public double Cantidad { get; set; }
        [DisplayName("Unidad de medida")]
        public string UnidadMedida { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Transportista { get; set; } = string.Empty;
        public string Tonel { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
    }
}
