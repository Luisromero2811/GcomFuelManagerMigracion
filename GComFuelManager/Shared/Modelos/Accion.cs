using System;
using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.Modelos
{
	public class Accion
	{
        [Key] public Int16? Cod { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool Estatus { get; set; } = true;
    }
}

