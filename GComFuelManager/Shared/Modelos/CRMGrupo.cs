namespace GComFuelManager.Shared.Modelos
{
    public class CRMGrupo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        public List<CRMGrupoRol> GrupoRols { get; set; } = new();
    }
}
