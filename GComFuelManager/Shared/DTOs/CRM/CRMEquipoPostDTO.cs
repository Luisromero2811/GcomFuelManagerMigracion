using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMEquipoPostDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int LiderId { get; set; }
        public int DivisionId { get; set; }
        public bool Activo { get; set; } = true;
        public List<CRMVendedorDTO> Vendedores { get; set; } = new();
    }
}
