using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class InventarioDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Producto { get; set; } = string.Empty;
        public string Sitio { get; set; } = string.Empty;
        public string Almacen { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
        public string TipoMovimiento { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public double Cantidad { get; set; }
        public string UnidadMedida { get; set; } = string.Empty;
        public DateTime FechaCierre { get; set; } = DateTime.Today;
    }
}
