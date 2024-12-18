namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMOportunidadPostDTO
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
        public DateTime? FechaGanada { get; set; }
        public DateTime? FechaConclusión { get; set; }
        public int EtapaVentaId { get; set; }
        public decimal Probabilidad { get; set; }
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
        public bool EsConclusion { get; set; }

        //Documento
        public int DocumentoId { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public DateTime FechaCaducidad { get; set; } = DateTime.Today;
        public string Descripcion { get; set; } = string.Empty;
        public int DocumentoRelacionado { get; set; }
        public int DocumentoRevision { get; set; }
        public List<int> TiposDocumentoIds { get; set; } = new();

        public CRMDocumentoDTO? DocumentoReciente { get; set; }
        public string? Comentarios { get; set; } = string.Empty;
    }
}
