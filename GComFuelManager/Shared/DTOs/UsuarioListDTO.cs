using System;
namespace GComFuelManager.Shared.DTOs
{
	public class UsuarioListDTO
	{
		public int Cod { get; set; } = 0;
		public string Den { get; set; } = string.Empty;
		public string Usuario { get; set; } = string.Empty;
		public string Contraseña { get; set; } = string.Empty;
		public DateTime FechaAlta { get; set; } = DateTime.MinValue;
		public bool Activo { get; set; } = false;
		public byte Tipo { get; set; } = 0;
	}
}

