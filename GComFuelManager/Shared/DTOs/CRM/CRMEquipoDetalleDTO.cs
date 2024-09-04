using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMEquipoDetalleDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int LiderId { get; set; }

        public CRMOriginador Originador { get; set; } = new();
        public List<CRMEquipoVendedor> EquipoVendedores { get; set; } = new();
        public List<CRMVendedorDTO> Vendedores { get; set; } = new();
    }
}
