namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMContactoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public int Cuenta { get; set; }
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public int Estatus { get; set; }
        public int Asignado { get; set; }
        public bool Activo { get; set; }
    }
}
