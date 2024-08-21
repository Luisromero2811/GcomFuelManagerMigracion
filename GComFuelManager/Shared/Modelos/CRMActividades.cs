using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMActividades
    {
        public int Id { get; set; }
        public int? Asunto { get; set; }
        public DateTime? Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime? Fecha_Mod { get; set; }
        public DateTime? Fecha_Inicio { get; set; } = DateTime.Now;
        public DateTime? Fecha_Fin { get; set; }
        public DateTime? Fecha_Ven { get; set; } = DateTime.Now;
        public int? Prioridad { get; set; }
        public int? Asignado { get; set; }
        public string? Desccripcion { get; set; } = string.Empty;
        public int? Estatus { get; set; }
        public int? Contacto_Rel { get; set; }
        public int? Recordatorio { get; set; }
        public bool Activo { get; set; } = true;

        public Catalogo_Fijo? asuntos { get; set; } = null!;
        public Catalogo_Fijo? prioridades { get; set; } = null!;
        public Catalogo_Fijo? Estados { get; set; } = null!;
        public Vendedor? vendedor { get; set; } = null!;
        public CRMContacto? contacto { get; set; } = null!;
    }
}
