using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;
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
        public double ReservadoDisponible { get; set; }
        public double Disponible { get; set; }
        public double PedidoTotal { get; set; }
        public double OrdenReservado { get; set; }
        public double EnOrden { get; set; }
        public double Cargado { get; set; }
        public double TotalDisponible { get; set; }
        public double TotalDisponibleFull { get; set; }
        public DateTime FechaCierre { get; set; } = DateTime.Now;
        public DateTime? FechaInicio { get; set; } = null!;
        public bool Abierto { get; set; } = true;
        public UsuarioSistemaDTO? UsuarioInicio { get; set; } = new();
        public UsuarioSistemaDTO? UsuarioCierre { get; set; } = new();
    }
}
