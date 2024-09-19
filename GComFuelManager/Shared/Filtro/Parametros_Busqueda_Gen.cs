using GComFuelManager.Shared.Interfaces;
using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Filtro
{
    public class Parametros_Busqueda_Gen : IPaginacionBusqueda
    {
        [NotMapped, EpplusIgnore, JsonIgnore] public int Pagina { get; set; } = 1;
        [NotMapped, EpplusIgnore, JsonIgnore] public int Pagina_ACtual { get; set; } = 1;
        [NotMapped, EpplusIgnore, JsonIgnore] public int Total_paginas { get; set; } = 1;
        [NotMapped, EpplusIgnore, JsonIgnore] public int Registros_por_pagina { get; set; } = 10;
        [NotMapped, EpplusIgnore, JsonIgnore] public int Total_registros { get; set; }
        [NotMapped, EpplusIgnore, JsonIgnore] public DateTime Fecha_Inicio { get; set; } = DateTime.Today;
        [NotMapped, EpplusIgnore, JsonIgnore] public DateTime Fecha_Fin { get; set; } = DateTime.Today;
        [NotMapped, EpplusIgnore, JsonIgnore] public bool Excel { get; set; } = false;
        //public T? Objeto { get; set; }
    }
}
