namespace GComFuelManager.Shared.Modelos
{
    public class CRMOportunidad
    {
        public int Id { get; set; }
        public string Nombre_Opor { get; set; } = string.Empty;
        public double ValorOportunidad { get; set; } = 0;
        public int UnidadMedidaId { get; set; }
        public string Prox_Paso { get; set; } = string.Empty;
        public int OrigenId { get; set; }
        public int VendedorId { get; set; }
        public int CuentaId { get; set; }
        public int TipoId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaCierre { get; set; }
        public int EtapaVentaId { get; set; }
        public decimal Probabilidad { get; set; }
        public bool Activo { get; set; } = true;

        public Catalogo_Fijo? UnidadMedida { get; set; } = null!;
        public Catalogo_Fijo? Origen { get; set; } = null!;
        public Vendedor? Vendedor { get; set; } = null!;
        public CRMCliente? CRMCliente { get; set; } = null!;
        public Catalogo_Fijo? Tipo { get; set; } = null!;
        public Catalogo_Fijo? EtapaVenta { get; set; } = null!;

    }
}
