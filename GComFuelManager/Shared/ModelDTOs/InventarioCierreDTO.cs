using GComFuelManager.Shared.Filtro;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class InventarioCierreDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Producto { get; set; } = string.Empty;
        public int ProductoId { get; set; }
        public string Sitio { get; set; } = string.Empty;
        public int SitioId { get; set; }
        public string Almacen { get; set; } = string.Empty;
        public int AlmacenId { get; set; }
        public string Localidad { get; set; } = string.Empty;
        public int LocalidadId { get; set; }
        public double Fisico { get; set; }
        public double Reservado { get; set; }
        public double Disponible { get; set; }
        public double PedidoTotal { get; set; }
        public double OrdenReservado { get; set; }
        public double EnOrden { get; set; }
        public double TotalDisponible { get; set; }
        public double TotalDisponibleFull { get; set; }
        public DateTime FechaCierre { get; set; } = DateTime.Now;
        //public string Terminal { get; set; } = string.Empty;
    }

    public class InventarioAnteriorNuevoCierreDTO
    {
        public InventarioCierreDTO Anterior { get; set; } = new();
        public InventarioCierreDTO Nuevo { get; set; } = new();
        public string Producto { get; set; } = string.Empty;
        public string Sitio { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
    }
}
