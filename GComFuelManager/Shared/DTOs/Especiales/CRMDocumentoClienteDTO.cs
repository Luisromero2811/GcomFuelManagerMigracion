using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.Especiales;
public class CRMDocumentoClienteDTO : Parametros_Busqueda_Gen
{
    public int Id { get; set; }
    public string NombreDocumento { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Actividad { get; set; } = string.Empty;
    public string Oportunidad { get; set; } = string.Empty;
    public string ClienteActividad { get; set; } = string.Empty;
    public string ClienteOportunidad { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime FechaCaducidad { get; set; } = DateTime.Now;
    public int OportunidadId { get; set; }
    public int ActividadId { get; set; }
    public string Url { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string VendedorActividad { get; set; } = string.Empty;
    public string VendedorOportunidad { get; set; } = string.Empty;
    public string? Comentarios { get; set; } = string.Empty;

    public List<string> TipoDocumento { get; set; } = new();
    public List<CRMDocumentoRevision> Revisiones { get; set; } = new();
}
