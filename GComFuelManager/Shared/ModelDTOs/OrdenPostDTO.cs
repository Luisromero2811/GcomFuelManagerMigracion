namespace GComFuelManager.Shared.ModelDTOs
{
    public class OrdenPostDTO
    {
        public int DestinoId { get; set; }
        public short ProductoId { get; set; }
        public int Volumen { get; set; }
        public DateTime FechaCarga { get; set; } = DateTime.Today;
        public DateTime FechaDeseadaCarga { get; set; } = DateTime.Today;
        public string Turno { get; set; } = string.Empty;
        public double Precio { get; set; }
    }
}
