using GComFuelManager.Shared.Enums;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class InventarioPostDTO
    {
        public int Id { get; set; }
        public int ProductoId { get; set; }
        public int SitioId { get; set; }
        public int AlmacenId { get; set; }
        public int LocalidadId { get; set; }
        public int TipoMovimientoId { get; set; }
        public string Referencia { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public double CantidadFacturada { get; set; }
        public double TirillaInicial { get; set; }
        public double TirillaFinal { get; set; }
        public double Temperatura { get; set; }
        public double Cantidad { get; set; }
        public double Diferencia { get; set; }
        public int UnidadMedidaId { get; set; }
        public int TransportistaId { get; set; }
        public int TonelId { get; set; }
        public int CierreId { get; set; }
        public TipoInventario TipoInventario { get; set; } = TipoInventario.Inicial;
        public int OrigenDestinoID { get; set; }
        public int ChoferId { get; set; }
        public DateTime FechaInicioMovimiento { get; set; } = DateTime.Now;
        public DateTime FechaFinMovimiento { get; set; } = DateTime.Now;
        public DateTime FechaMovimiento { get; set; } = DateTime.Today;
    }
}
