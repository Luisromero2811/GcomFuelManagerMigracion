namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMDocumentoDetalleDTO
    {
        public int Id { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaCaducidad { get; set; } = DateTime.Today;
        public string Descripcion { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string VersionCreadaPor { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public CRMDocumentoDTO? DocumentoRelacionado { get; set; } = new();
        public CRMDocumentoDTO? DocumentoRevision { get; set; } = new();
    }
}
