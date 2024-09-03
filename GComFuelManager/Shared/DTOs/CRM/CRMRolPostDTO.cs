namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMRolPostDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DivisionId { get; set; }
        public bool Activo { get; set; }
        public List<CRMPermisoDTO> Permisos { get; set; } = new();
    }
}
