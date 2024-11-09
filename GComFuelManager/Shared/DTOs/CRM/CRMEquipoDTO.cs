using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMEquipoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public List<CRMOriginadorDTO> Lideres { get; set; } = new();
        public string Lider { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public int VendedorId { get; set; }
    }
}
