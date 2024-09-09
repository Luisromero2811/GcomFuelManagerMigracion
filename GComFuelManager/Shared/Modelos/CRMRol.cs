namespace GComFuelManager.Shared.Modelos
{
    public class CRMRol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int DivisionId { get; set; }
        public bool Activo { get; set; } = true;

        public CRMDivision? Division { get; set; } = null!;
    }
}
