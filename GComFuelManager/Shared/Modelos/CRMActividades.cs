using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.DTOs.CRM;

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
        public int? EquipoId { get; set; }

        public Catalogo_Fijo? asuntos { get; set; } = null!;
        public Catalogo_Fijo? prioridades { get; set; } = null!;
        public Catalogo_Fijo? Estados { get; set; } = null!;
        public CRMVendedor? vendedor { get; set; } = null!;
        public CRMContacto? contacto { get; set; } = null!;
        public CRMEquipo? Equipo { get; set; } = null!;

        //Documentos
        public List<CRMDocumento> Documentos { get; set; } = new();
        public List<CRMActividadDocumento> ActividadDocumentos { get; set; } = new();

        public CRMActividadExcelDTO Asignacion_Datos()
        {
            CRMActividadExcelDTO gestion_ = new();

            gestion_.Contacto_Rel = (contacto?.Nombre ?? "") + " " + (contacto?.Apellidos ?? "");
            gestion_.Asunto = asuntos?.Valor;
            gestion_.Prioridad = prioridades?.Valor;
            gestion_.Fch_Inicio = Fch_Inicio;
            gestion_.Fecha_Ven = Fecha_Ven;
            gestion_.Estatus = Estados?.Valor;
            gestion_.Asignado = (vendedor?.Nombre ?? "") + " " + (vendedor?.Apellidos ?? "");
            gestion_.Desccripcion = Desccripcion;

            return gestion_;
        }
    }
}
