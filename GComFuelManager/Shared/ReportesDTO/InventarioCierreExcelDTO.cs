using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class InventarioCierreExcelDTO
    {
        [DisplayName("Fecha de cierre")]
        public DateTime? FechaCierre { get; set; } = null;
        public string Producto { get; set; } = string.Empty;
        public string Sitio { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        [DisplayName("Inventario físico")]
        public double Fisico { get; set; }
        [DisplayName("Física reservada")]
        public double Reservado { get; set; }
        [DisplayName("Física disponible")]
        public double Disponible { get; set; }
        [DisplayName("Pedido en total")]
        public double PedidoTotal { get; set; }
        [DisplayName("Ordenada reservada")]
        public double OrdenReservado { get; set; }
        [DisplayName("En orden")]
        public double EnOrden { get; set; }
        public double Cargado { get; set; }
        [DisplayName("Total dispobible")]
        public double TotalDisponible { get; set; }
        [DisplayName("Total dispoble en full")]
        public double TotalDisponibleFull { get; set; }
    }
}
