namespace GComFuelManager.Shared.Modelos
{
    public class CRMActividad
    {
        public int Id { get; set; }
        public int Asunto { get; set; }
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime Fecha_Mod { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }
        public DateTime Fecha_Ven { get; set; }
        public int Prioridad { get; set; }
        public int Asignado { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Estatus { get; set; }
        public int Contacto_Rel { get; set; }
        public int Recordatorio { get; set; }
        public bool Activo { get; set; } = true;

        public Catalogo_Fijo? catalogo_Fijo { get; set; } = null!;
        public Vendedor? vendedor { get; set; } = null!;
        public CRMContacto? contacto { get; set; } = null!;
    }
}
