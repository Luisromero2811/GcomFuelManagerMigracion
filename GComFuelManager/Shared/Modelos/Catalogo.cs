using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Catalogo
    {
        [Key]
        public int Cod { get; set; }
        public string Den { get; set; } = string.Empty;
        public int CodAcc { get; set; } = 0;
    }
}
