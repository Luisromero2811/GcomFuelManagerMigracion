using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMRolDetalleDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public CRMDivision Division { get; set; } = null!;
        public List<CRMPermisoDTO> Permisos { get; set; } = new();
    }
}
