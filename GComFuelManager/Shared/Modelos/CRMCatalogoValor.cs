namespace GComFuelManager.Shared.Modelos
{
    public class CRMCatalogoValor
    {
        public int Id { get; set; }
        public string Valor { get; set; } = string.Empty;
        public string? Abreviacion { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public int CatalogoId { get; set; }
        public CRMCatalogo Catalogo { get; set; } = null!;
    }
}
