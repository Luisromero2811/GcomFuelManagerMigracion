namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMGrupoPermisoDTO
    {
        public string Grupo { get; set; } = string.Empty;
        public List<CRMPermisoDTO> Permisos { get; set; } = new();
    }
}
