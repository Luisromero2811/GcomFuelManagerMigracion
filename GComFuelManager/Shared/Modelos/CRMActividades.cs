using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.DTOs.CRM;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
    public class CRMActividades
    {
        public int Id { get; set; }
        public int Asunto { get; set; }
        public DateTime? Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime? Fecha_Mod { get; set; }
        public DateTime? Fch_Inicio { get; set; } = DateTime.Now;
        public DateTime? Fecha_Ven { get; set; } = DateTime.Now;
        public int? Prioridad { get; set; }
        public int? Asignado { get; set; }
        public string? Desccripcion { get; set; } = string.Empty;
        public int? Estatus { get; set; }
        public int Contacto_Rel { get; set; }
        public int? Recordatorio { get; set; }
        public bool Activo { get; set; } = true;
        public int EquipoId { get; set; }
        public string? Retroalimentacion { get; set; } = string.Empty;
        [NotMapped]
        public List<int> TiposDocumentoIds { get; set; } = new();

        public CRMCatalogoValor Asuntos { get; set; } = null!;
        public CRMCatalogoValor? Prioridades { get; set; } = null!;
        public CRMCatalogoValor? Estados { get; set; } = null!;
        public CRMVendedor? Vendedor { get; set; } = null!;
        public CRMContacto Contacto { get; set; } = null!;
        public CRMEquipo Equipo { get; set; } = null!;
        [NotMapped]
        public CRMDocumento? Documento { get; set; } = null!;

        //Documentos
        public List<CRMDocumento> Documentos { get; set; } = new();
        public List<CRMActividadDocumento> ActividadDocumentos { get; set; } = new();


        public CRMActividadExcelDTO Asignacion_Datos()
        {
            CRMActividadExcelDTO gestion_ = new();

            gestion_.Contacto_Rel = (Contacto?.Nombre ?? "") + " " + (Contacto?.Apellidos ?? "");
            gestion_.Asunto = Asuntos?.Valor;
            gestion_.Prioridad = Prioridades?.Valor;
            gestion_.Fecha_Creacion = (DateTime)Fecha_Creacion;
            gestion_.Fch_Inicio = Fch_Inicio;
            gestion_.Fecha_Ven = Fecha_Ven;
            gestion_.Fecha_Mod = Fecha_Mod;
            gestion_.Estatus = Estados?.Valor;
            gestion_.Asignado = (Vendedor?.Nombre ?? "") + " " + (Vendedor?.Apellidos ?? "");
            gestion_.Desccripcion = Desccripcion;
            gestion_.Retroalimentacion = Retroalimentacion;

            return gestion_;
        }
    }
}
