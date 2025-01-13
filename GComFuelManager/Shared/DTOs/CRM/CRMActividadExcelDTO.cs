using System;
using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs.CRM
{
	public class CRMActividadExcelDTO
	{
        [DisplayName("Vendedor asignado")]
        public string? Asignado { get; set; } = string.Empty;
        //Cuenta
        [DisplayName("Cuenta Relacionada")]
        public string? Cuenta_Rel { get; set; } = string.Empty;
        public string? Asunto { get; set; } = string.Empty;
        [DisplayName("Contacto relacionado")]
        public string? Contacto_Rel { get; set; } = string.Empty;
        public string? Prioridad { get; set; } = string.Empty;
        [DisplayName("Descripción")]
        public string? Desccripcion { get; set; } = string.Empty;
        [DisplayName("Fecha de Creación")]
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        [DisplayName("Fecha de Inicio")]
        public DateTime? Fch_Inicio { get; set; } = DateTime.Now;
        [DisplayName("Fecha de Vencimiento")]
        public DateTime? Fecha_Ven { get; set; } = DateTime.Now;
        [DisplayName("Fecha de Modificación")]
        public DateTime? Fecha_Mod { get; set; } = DateTime.Now;
        [DisplayName("Estatus de Actividad")]
        public string? Estatus { get; set; } = string.Empty;
        [DisplayName("Retroalimentación")]
        public string? Retroalimentacion { get; set; } = string.Empty;
    }
}

