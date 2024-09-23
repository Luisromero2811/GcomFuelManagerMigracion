namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMClientePostDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ContactoPrincipalId { get; set; }
    }
}
