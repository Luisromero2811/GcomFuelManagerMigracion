using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Autorizador
    {
        [Key]
        public int Cod { get; set; }
        public string Den { get; set; } = string.Empty;
        public short Id_Tad { get; set; }
    }
}
