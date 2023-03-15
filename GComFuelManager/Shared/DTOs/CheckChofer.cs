using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class CheckChofer
    {
        public int? Chofer { get; set; } = 0;
        public int? Tonel { get; set; } = 0;
        public int? Compartimento { get; set; } = 0;
        public DateTime? FechaCarga { get; set; } = DateTime.Today;
    }
}
