using System;
using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs.CRM
{
	public class CRMTipoDocumentoDTO : Parametros_Busqueda_Gen
    {
        public int Id { get; set; }
        public string? Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
    }
}

