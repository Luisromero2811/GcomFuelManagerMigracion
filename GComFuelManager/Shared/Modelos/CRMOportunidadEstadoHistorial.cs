namespace GComFuelManager.Shared.Modelos
{
    public class CRMOportunidadEstadoHistorial
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public int OportunidadId { get; set; }
        public int EtapaVentaId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime FechaCambio { get; set; } = DateTime.Now;

        public CRMOportunidad Oportunidad { get; set; } = null!;
        public CRMCatalogoValor EtapaVenta { get; set; } = null!;
    }
}
