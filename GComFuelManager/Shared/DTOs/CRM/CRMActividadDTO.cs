using System;
using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
    public class CRMActividadDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string? Asunto { get; set; } = string.Empty;
        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime? Fecha_Mod { get; set; } = DateTime.Now;
        public DateTime? Fch_Inicio { get; set; } = DateTime.Now;
        public DateTime Fecha_Ven { get; set; } = DateTime.Now;
        public string? Prioridad { get; set; } = string.Empty;
        public int Asignado { get; set; }
        public string? Desccripcion { get; set; } = string.Empty;
        public string? Estatus { get; set; } = string.Empty;
        public string? Contacto_Rel { get; set; } = string.Empty;
        public string? Cuenta_Rel { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public string? VendedorId { get; set; } = string.Empty;
        public string? Retroalimentacion { get; set; } = string.Empty;
    }
}

