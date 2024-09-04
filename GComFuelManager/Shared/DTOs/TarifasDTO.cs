using System;
namespace GComFuelManager.Shared.DTOs
{
	public class TarifasDTO
	{
        public DateTime FchIni { get; set; } = DateTime.Today.Date;
        public DateTime FchFin { get; set; } = DateTime.Now;
    }
}

