namespace GComFuelManager.Shared.Modelos
{
    public class CRMContacto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public int Cuenta { get; set; }
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Tel_Movil { get; set; } = string.Empty;
        public string SitioWeb { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CP { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        public int Estatus { get; set; }
        public string Estatus_Desc { get; set; } = string.Empty;
        public double Importe_Oportunidad { get; set; }
        public string Recomen { get; set; } = string.Empty;
        public int Asignado { get; set; }
        public bool Activo { get; set; }
    }
}
