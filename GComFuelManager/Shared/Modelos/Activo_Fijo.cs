using GComFuelManager.Shared.Filtro;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Activo_Fijo : Parametros_Busqueda_Gen
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
        public bool Activo { get; set; } = true;
        public int Numeracion { get; set; }
        [StringLength(250)]
        public string Nro_Etiqueta { get; set; } = string.Empty;
        public int Etiquetado_Activo { get; set; }
        public int Origen_Activo { get; set; }

        public Catalogo_Fijo Conjunto { get; set; } = new();
        public Catalogo_Fijo Condicion { get; set; } = new();
        public Catalogo_Fijo Tipo { get; set; } = new();
        public Catalogo_Fijo Unidad { get; set; } = new();
        public Catalogo_Fijo Origen { get; set; } = new();
        public Catalogo_Fijo Etiqueta { get; set; } = new();
    }

    public class Activos_Fijos_Excel
    {
        [StringLength(250)]
        public string Nombre { get; set; } = string.Empty;
        [StringLength(250)]
        public string Origen { get; set; } = string.Empty;

        [StringLength(250), DisplayName("Número de activo fijo")]
        public string Nro_Activo { get; set; } = string.Empty;

        [StringLength(250), DisplayName("Conjunto de item")]
        public string Conjunto { get; set; } = string.Empty;
        [StringLength(250), DisplayName("Condicion de activo")]
        public string Condicion { get; set; } = string.Empty;
        [StringLength(250)]
        public string Tipo { get; set; } = string.Empty;
        [StringLength(250), DisplayName("Unidad de medida")]
        public string Unidad { get; set; } = string.Empty;
        [StringLength(250), DisplayName("No. Etiqueta")]
        public string Nro_Etiqueta { get; set; } = string.Empty;
        [StringLength(250)]
        public string Etiquetado { get; set; } = string.Empty;
    }
}
