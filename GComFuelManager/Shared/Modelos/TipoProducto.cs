using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class TipoProducto
    {
        public short Id { get; set; }

        [StringLength(30)] 
        public string Tipo { get; set; } = string.Empty;
    }
}
