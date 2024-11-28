using GComFuelManager.Shared.Enums;
using GComFuelManager.Shared.Filtro;
using OfficeOpenXml.Attributes;

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
        public DateTime FechaInicioMovimiento { get; set; } = DateTime.Now;
        public DateTime FechaFinMovimiento { get; set; } = DateTime.Now;
        public double CantidadFacturada { get; set; }
        public double TirillaInicial { get; set; }
        public double TirillaFinal { get; set; }
        public double Temperatura { get; set; }
        public double Cantidad { get; set; }
        public double Diferencia { get; set; }
        public int CierreId { get; set; }
        public string Transportista { get; set; } = string.Empty;
        public string Tonel { get; set; } = string.Empty;
        public string Chofer { get; set; } = string.Empty;
        public TipoInventario TipoInventario { get; set; }
        public string OrigenDestino { get; set; } = string.Empty;
        public DateTime FechaMovimiento { get; set; } = DateTime.Now;
        [EpplusIgnore] public bool FechaNULL { get; set; } = false;

        public string UnidadMedida { get; set; } = string.Empty;
        public int UnidadMedidaId { get; set; }
        public DateTime? FechaCierre { get; set; } = null;
        public DateTime FechaRegistro { get; set; } = DateTime.Today;

    }
}
