namespace GComFuelManager.Shared.Modelos
{
    public class CRMCatalogo
    {
        public int Id { get; set; }
        public string Clave { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public List<CRMCatalogoValor> Valores { get; set; } = new();
    }
}
