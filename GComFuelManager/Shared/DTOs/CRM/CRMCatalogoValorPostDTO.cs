namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMCatalogoValorPostDTO
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string? Abreviacion { get; set; } = string.Empty;
        public int CatalogoId { get; set; }
    }
}
