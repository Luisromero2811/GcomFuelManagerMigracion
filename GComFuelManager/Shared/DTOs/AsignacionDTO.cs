using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class AsignacionDTO
    {
        public int? CodChf { get; set; }
        public int? CodTra { get; set; }
        public int? CodTon { get; set; }
        public int Compartimiento { get; set; } = 1;
    }
}
