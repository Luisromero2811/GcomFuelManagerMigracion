namespace GComFuelManager.Shared.ModelDTOs;

public class UsuarioSistemaDTO
{
    public int Cod { get; set; }
    public string Den { get; set; } = string.Empty;

    public override string ToString()
    {
        return Den;
    }
}

