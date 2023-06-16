using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Shared.DTOs
{
    public class CodCteDTO
    {
        public string? CodCte { get; set; } = string.Empty;
        public Cliente? cliente { get; set; }
    }
}