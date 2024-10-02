using System;
using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs.Reportes.CRM
{
	public class CRMActividadesExcelDTO
	{
        [DisplayName("Contacto Relacionado")]
        public string Contacto_Rel { get; set; } = string.Empty;
        [DisplayName("Asunto")]
        public string Asunto { get; set; } = string.Empty;
        [DisplayName("Prioridad")]
        public string Prioridad { get; set; } = string.Empty;
        [DisplayName("Estado de actividad")]
        public string Estatus { get; set; } = string.Empty;
        [DisplayName("Fecha de Inicio")]
        public DateTime? Fch_Inicio { get; set; } = DateTime.Now;
        [DisplayName("Fecha de Vencimiento")]
        public DateTime? Fecha_Ven { get; set; } = DateTime.Now;
        [DisplayName("Vendedor")]
        public string Asignado { get; set; } = string.Empty;
        [DisplayName("Descripción")]
        public string Desccripcion { get; set; } = string.Empty;
        
      
    }
}

