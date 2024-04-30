using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Activo_Fijo
    {
        public int Id { get; set; }
        [StringLength(250)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(250)]
        public string Nro_Activo { get; set; } = string.Empty;
        public int Conjunto_Activo { get; set; }
        public int Condicion_Activo { get; set; }
        public int Unidad_Medida { get; set; }
        public int Tipo_Activo { get; set; }

        public Catalogo_Fijo Conjunto { get; set; } = new();
        public Catalogo_Fijo Condicion { get; set; } = new();
        public Catalogo_Fijo Tipo { get; set; } = new();
        public Catalogo_Fijo Unidad { get; set; } = new();
    }
}
