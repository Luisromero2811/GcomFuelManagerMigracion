using GComFuelManager.Shared.Filtro;
using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class InventarioDTO : Parametros_Busqueda_Gen
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
        public string TipoMovimiento { get; set; } = string.Empty;
        public int TipoMovimientoId { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public int CierreId { get; set; }
        [EpplusIgnore] public bool FechaNULL { get; set; } = false;

        public string UnidadMedida { get; set; } = string.Empty;
        public int UnidadMedidaId { get; set; }
        public DateTime? FechaCierre { get; set; } = null;
        public DateTime FechaRegistro { get; set; } = DateTime.Today;

    }
}
