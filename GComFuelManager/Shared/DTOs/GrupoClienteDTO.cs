using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs
{
	public class GrupoClienteDTO
	{
		public Cliente? cliente { get; set; }
		public Grupo? grupo { get; set; }
	}
}

