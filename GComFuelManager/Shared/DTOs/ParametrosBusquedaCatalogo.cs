using System;
namespace GComFuelManager.Shared.DTOs
{
	public class ParametrosBusquedaCatalogo
	{
		//Chofer
		public string nombrechofer { get; set; } = string.Empty!;
		public string apellidochofer { get; set; } = string.Empty!;
		public int busentid { get; set; } = 0;
		public int codtransport { get; set; } = 0;

		//Unidad-Tonel
		public string placatonel { get; set; } = string.Empty!;
		public int carrid { get; set; } = 0;

		//Cliente
		public string nombrecliente { get; set; } = string.Empty!;
		public int codgru { get; set; } = 0;

		//Destino
		public string nombredestino { get; set; } = string.Empty!;
		public int codcte { get; set; } = 0;
     }
}

