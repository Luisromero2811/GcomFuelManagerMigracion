namespace GComFuelManager.Shared.ModelDTOs
{
    public class CatalogoValorPostDTO
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string Abreviacion { get; set; } = string.Empty;
        public int CatalogoId { get; set; }
    }
}
