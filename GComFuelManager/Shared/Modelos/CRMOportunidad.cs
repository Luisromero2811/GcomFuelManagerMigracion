namespace GComFuelManager.Shared.Modelos
{
    public class CRMOportunidad
    {
        public int Id { get; set; }
        public string Nombre_Opor { get; set; } = string.Empty;
        public double ValorOportunidad { get; set; } = 0;
        public int UnidadMedidaId { get; set; }
        public string Prox_Paso { get; set; } = string.Empty;
        public int VendedorId { get; set; }
        public int CuentaId { get; set; }
        public int ContactoId { get; set; }
        public int PeriodoId { get; set; }
        public int TipoId { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaCierre { get; set; } = DateTime.Today;
        public int EtapaVentaId { get; set; }
        public decimal Probabilidad { get; set; }
        public bool Activo { get; set; } = true;
        public int OrigenPrductoId { get; set; }
        public int TipoProductoId { get; set; }
        public int ModeloVentaId { get; set; }
        public int VolumenId { get; set; }
        public int FormaPagoId { get; set; }
        public int DiasPagoId { get; set; }
        public int CantidadEstaciones { get; set; }
        public double CantidadLts { get; set; }
        public double PrecioLts { get; set; }
        public double TotalLts { get; set; }
        public int EquipoId { get; set; }
        public CRMCatalogoValor UnidadMedida { get; set; } = null!;
        public CRMVendedor Vendedor { get; set; } = null!;
        public CRMCliente CRMCliente { get; set; } = null!;
        public CRMCatalogoValor Tipo { get; set; } = null!;
        public CRMCatalogoValor EtapaVenta { get; set; } = null!;
        public CRMContacto Contacto { get; set; } = null!;
        public CRMCatalogoValor Periodo { get; set; } = null!;
        public CRMCatalogoValor OrigenProducto { get; set; } = null!;
        public CRMCatalogoValor TipoProducto { get; set; } = null!;
        public CRMCatalogoValor ModeloVenta { get; set; } = null!;
        public CRMCatalogoValor Volumen { get; set; } = null!;
        public CRMCatalogoValor FormaPago { get; set; } = null!;
        public CRMCatalogoValor? DiasCredito { get; set; } = null!;
        public CRMEquipo Equipo { get; set; } = null!;
        public List<CRMDocumento> Documentos { get; set; } = new();
        public List<CRMOportunidadDocumento> OportunidadDocumentos { get; set; } = new();
    }
}
