using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class InventarioConsolidacionDTO
    {
        public string Producto { get; set; } = string.Empty;
        [DisplayName("Inventario físico")]
        public double Fisico { get; set; }
        [DisplayName("Física reservada")]
        public double Reservado { get; set; }
        [DisplayName("Física disponible")]
        public double Disponible { get; set; }
        [DisplayName("Física reservada disponible")]
        public double ReservadoDisponible { get; set; }
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
