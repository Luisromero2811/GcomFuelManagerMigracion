using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
	public class Autorizadores_Tad
	{
        public int Id_Autorizador { get; set; }
        public short Id_Terminal { get; set; }

        [NotMapped] public Autorizador? Autorizador { get; set; } = null!;
        [NotMapped] public Tad? Terminal { get; set; } = null!;
    }
}

