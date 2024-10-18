namespace GComFuelManager.Shared.ModelDTOs
{
    public class InventarioCierreDTO
    {
        public int Id { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string UnidadMedida { get; set; } = string.Empty;
        public string Sitio { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        public double Fisico { get; set; }
        public double Reservado { get; set; }
        public double Disponible { get; set; }
        public double PedidoTotal { get; set; }
        public double OrdenReservado { get; set; }
        public double EnOrden { get; set; }
        public double TotalDisponible { get; set; }
        public DateTime FechaCierre { get; set; } = DateTime.Now;
        public string Terminal { get; set; } = string.Empty;
    }

    public class InventarioAnteriorNuevoCierreDTO
    {
        public InventarioCierreDTO Anterior { get; set; } = new();
        public InventarioCierreDTO Nuevo { get; set; } = new();
    }
}
