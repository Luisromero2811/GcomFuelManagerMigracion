namespace GComFuelManager.Shared.Modelos;

public class Catalogo
{
    public int Id { get; set; }
    public string Clave { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;

    public List<CatalogoValor> Valores { get; set; } = new();
    public List<Catalogo_Fijo> ValoresFijos { get; set; } = new();
}
