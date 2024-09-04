namespace GComFuelManager.Shared.Modelos
{
    public class CRMCliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Estado { get; set; }
        public string Tel { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}
