namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMContactoDTO
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        //public int IdCuenta { get; set; }
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estatus { get; set; } = string.Empty;
        //public int Estatus { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        //public int IdAsignado { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public bool Activo { get; set; }

        public string FullName
        {
            get
            {
                return $"{Apellidos} {Nombre}";
            }
        }
    }
}
