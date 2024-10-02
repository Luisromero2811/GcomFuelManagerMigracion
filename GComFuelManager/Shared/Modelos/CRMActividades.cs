using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMActividades
    {
        public int Id { get; set; }
        public int? Asunto { get; set; }
        public DateTime? Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime? Fecha_Mod { get; set; }
        public DateTime? Fch_Inicio { get; set; } = DateTime.Now;
        public DateTime? Fecha_Ven { get; set; } = DateTime.Now;
        public int? Prioridad { get; set; }
        public int? Asignado { get; set; }
        public string? Desccripcion { get; set; } = string.Empty;
        public int? Estatus { get; set; }
        public int? Contacto_Rel { get; set; }
        public int? Recordatorio { get; set; }
        public bool Activo { get; set; } = true;

        public CRMCatalogoValor? asuntos { get; set; } = null!;
        public CRMCatalogoValor? prioridades { get; set; } = null!;
        public CRMCatalogoValor? Estados { get; set; } = null!;
        public CRMVendedor? vendedor { get; set; } = null!;
        public CRMContacto? contacto { get; set; } = null!;
    }
}
