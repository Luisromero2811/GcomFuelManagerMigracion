using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMCatalogoValorDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string? Abreviacion { get; set; } = string.Empty;
        public int CatalogoId { get; set; }
    }
}
