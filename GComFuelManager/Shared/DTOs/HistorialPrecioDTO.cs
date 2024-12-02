using System.ComponentModel;


namespace GComFuelManager.Shared.DTOs
{
    public class HistorialPrecioDTO
    {
        [DisplayName("Fecha")]
        public string? Fecha { get; set; } = string.Empty;

        [DisplayName("Precio")]
        public double? Pre { get; set; } = 0;
        [DisplayName("Precio de compra")]
        public double? PrecioCompra { get; set; } = 0;
        public string? Producto { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Zona { get; set; } = string.Empty;
        public string? Cliente { get; set; } = string.Empty;
        public string? Moneda { get; set; } = string.Empty;
        public double? Cambio { get; set; } = 1;
        public string? Usuario { get; set; } = string.Empty;

        [DisplayName("Fecha de subida")]
        public string? Fecha_De_Subida { get; set; } = string.Empty;

        [DisplayName("Unidad de negocio")]
        public string? Unidad_Negocio { get; set; } = string.Empty;
    }
}

