using System;
using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs.Reportes.CRM
{
	public class CRMContactosExcelDTO
	{
        [DisplayName("Nombre del Contacto")]
        public string Nombre { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        [DisplayName("Telefono Móvil")]
        public string Tel_Movil { get; set; } = string.Empty;
        [DisplayName("Telefono Oficina")]
        public string Tel_Oficina { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Vendedor { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string? Estado { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public string Calle { get; set; } = string.Empty;
        public string Colonia { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string CP { get; set; } = string.Empty;
        public string Pais { get; set; } = string.Empty;
        [DisplayName("Sitio Web")]
        public string SitioWeb { get; set; } = string.Empty;
        [DisplayName("Recomendacion")]
        public string Recomen { get; set; } = string.Empty;

    }
}

