using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMDocumentoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string NombreDocumento { get; set; } = string.Empty;
        public string TipoDocumento { get; set; } = string.Empty;
        public DateTime FechaCaducidad { get; set; } = DateTime.Now;
        public string Version { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int OportunidadId { get; set; }
        public int ActividadId { get; set; }
        public byte[] InfoBytes { get; set; } = Array.Empty<byte>();
    }
}
