using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMDocumento
    {
        public int Id { get; set; }
        public string NombreArchivo { get; set; } = string.Empty;
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
        public DateTime FechaCaducidad { get; set; } = DateTime.Today;
        public string Descripcion { get; set; } = string.Empty;
        //public int DocumentoRelacionado { get; set; }
        public bool Activo { get; set; } = true;
        public string Version { get; set; } = string.Empty;
        //public int RevisionRelacionada { get; set; }
        public string VersionCreadaPor { get; set; } = string.Empty;
        public string Directorio { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string Identificador { get; set; } = string.Empty;

        public List<CRMOportunidad> Oportunidades { get; set; } = new();
        public List<CRMOportunidadDocumento> OportunidadDocumentos { get; set; } = new();


        public List<CRMActividades> Actividades { get; set; } = new();
        public List<CRMActividadDocumento> ActividadDocumentos { get; set; } = new();

        public List<TipoDocumento> TipoDocumentos { get; set; } = new();
        public List<DocumentoTipoDocumento> DocumentoTipoDocumentos { get; set; } = new();
        public string? Comentarios { get; set; } = string.Empty;

        public List<ConoceClienteOportunidad> conoceClienteOportunidades { get; set; } = new();
        public List<CRMConoceClienteDocumentos> CRMConoceClienteDocumentos { get; set; } = new();
 
        
    }
}
