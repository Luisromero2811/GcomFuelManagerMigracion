using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMDocumentoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int OportunidadId { get; set; }
        public int ActividadId { get; set; }
    }
}
