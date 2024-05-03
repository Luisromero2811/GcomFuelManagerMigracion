using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Filtro
{
    public class Parametros_Busqueda_Gen
    {
        [NotMapped, EpplusIgnore] public int Pagina { get; set; } = 1;
        [NotMapped, EpplusIgnore] public int Pagina_ACtual { get; set; } = 1;
        [NotMapped, EpplusIgnore] public int Total_paginas { get; set; } = 1;
        [NotMapped, EpplusIgnore] public int Registros_por_pagina { get; set; } = 10;
        [NotMapped, EpplusIgnore] public int Total_registros { get; set; }
        //public T? Objeto { get; set; }
    }
}
