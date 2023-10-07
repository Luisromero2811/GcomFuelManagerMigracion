using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Errors
    {

        [Key] 
        public string Cod { get; set; } = new Guid().ToString();
        public Error Error { get; set; } = new Error();
        public DateTime Fch { get; set; } = DateTime.Now;
        public string Accion { get; set; } = string.Empty;
    }

    public class Error
    {
        public string Inner { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
