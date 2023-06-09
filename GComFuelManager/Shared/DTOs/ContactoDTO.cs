using GComFuelManager.Shared.Modelos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class ContactoDTO:Contacto
    {
        [NotMapped] public IEnumerable<AccionCorreo> Acciones { get; set; } = new List<AccionCorreo>();
    }
}
