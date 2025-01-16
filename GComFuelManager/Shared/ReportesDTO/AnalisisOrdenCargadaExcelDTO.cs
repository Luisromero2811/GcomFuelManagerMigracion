using GComFuelManager.Shared.Enums;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class AnalisisOrdenCargadaExcelDTO
    {
        public string Terminal { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public TipoVenta TipoVenta { get; set; } = TipoVenta.Externo;
        public string Producto { get; set; } = string.Empty;
        public double Volumen { get; set; } = 0;
        public string ImporteCompra { get; set; } = string.Empty;
        public double PrecioCompra { get; set; } = 0;
        public string BOL { get; set; } = string.Empty;
        public string Factura { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; } = DateTime.MinValue;
        public string Transportista { get; set; } = string.Empty;
        public string Operador { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public double PrecioVenta { get; set; } = 0;
        public string NumeroOrden { get; set; } = string.Empty;
    }
}
