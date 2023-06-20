using System;
using System.Text;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using GComFuelManager.Shared.Modelos;
using Newtonsoft.Json;

namespace GComFuelManager.Shared.DTOs
{
	public class ZonaDTO
	{
		public int? ZonaCod { get; set; }
		public Grupo? grupo { get; set; }
		public int? CteCod { get; set; }
		public int? DesCod { get; set; }
        
    }
}

