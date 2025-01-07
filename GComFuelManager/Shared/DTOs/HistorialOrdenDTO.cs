using GComFuelManager.Shared.Filtros;

namespace GComFuelManager.Shared.DTOs
{
    public class HistorialOrdenDTO : Parametros_Busqueda_Gen
    {
        public string Terminal { get; set; } = string.Empty;
        public DateTime FechaCarga { get; set; } = DateTime.Now;
        public string? Folio { get; set; } = string.Empty;
        public string BOL { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        public string Producto { get; set; } = string.Empty;
        public double? Volumen { get; set; }
        public double? VolumenCargado { get; set; }
        public string Transportista { get; set; } = string.Empty;
        public string Tonel { get; set; } = string.Empty;
        public string Chofer { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
