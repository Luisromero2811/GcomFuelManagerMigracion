namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMClienteDetalleDTO
    {
        public string Nombre { get; set; } = string.Empty;
        public CRMContactoDTO? Contacto { get; set; } = null!;
        
    }
}
