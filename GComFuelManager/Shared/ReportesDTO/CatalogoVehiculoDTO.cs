namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoVehiculoDTO
    {
        public string Placa { get; set; } = string.Empty;
        public string Tracto { get; set; } = string.Empty;
        public string? Placatracto { get; set; } = string.Empty;
        public decimal Capcom { get; set; }
        public decimal Capcom2 { get; set; }
        public decimal Capcom3 { get; set; }
        public int Capcom4 { get; set; }
        public string Trasnportista { get; set; } = string.Empty;
    }
}
