using System;
using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.Modelos
{
	public class Catalogo_Fijo
	{
        public int Id { get; set; }
        [StringLength(250)]
        public string Valor { get; set; } = string.Empty;
        public short Catalogo { get; set; }
        public bool Activo { get; set; } = true;
        public string? Abreviacion { get; set; } = string.Empty;
    }
}

