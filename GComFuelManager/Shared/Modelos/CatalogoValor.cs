namespace GComFuelManager.Shared.Modelos;
public class CatalogoValor : IEquatable<CatalogoValor>
{
    public int Id { get; set; }
    public string Valor { get; set; } = string.Empty;
    public string? Abreviacion { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public int CatalogoId { get; set; }
    public short TadId { get; set; }
    public bool EsEditable { get; set; } = true;
    public int Code { get; set; }
    public override string ToString()
    {
        return Valor;
    }

    public override bool Equals(object? obj) => Equals(obj);

    public override int GetHashCode() => (Valor, Abreviacion, TadId).GetHashCode();

    public bool Equals(CatalogoValor? other)
    {
        if (other is null) return false;
        return Valor.Equals(other.Valor) && Abreviacion == other.Abreviacion;
    }
}
