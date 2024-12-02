using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoVehiculoDTO
    {
        public string Tracto { get; set; } = string.Empty;
        public string Placa { get; set; } = string.Empty;
        public string? Placatracto { get; set; } = string.Empty;
        [DisplayName("Capacidad compartimiento 1")]
        public decimal Capcom { get; set; }
        [DisplayName("Capacidad compartimiento 2")]
        public decimal Capcom2 { get; set; }
        [DisplayName("Capacidad compartimiento 3")]
        public decimal Capcom3 { get; set; }
        [DisplayName("Capacidad compartimiento 4")]
        public int Capcom4 { get; set; }
        public string Transportista { get; set; } = string.Empty;
    }
}
