using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.DTOs
{
    public class CierreFiltroDTO
    {
        public string Folio { get; set; } = string.Empty;
        public int Bol { get; set; }
        public DateTime? FchInicio { get; set; } = DateTime.Now;
        public DateTime? FchFin{ get; set; } = DateTime.Now;
        public int? codCte { get; set; }
        public short? codGru { get; set; }
        public bool forFolio { get; set; } = false;
        public bool byMonth { get; set; } = false;
        [EpplusIgnore]
        public DateTime Month { get; set; } = DateTime.Now;
        [EpplusIgnore]
        public DateTime Year { get; set; } = DateTime.Now;
        public DateTime Fecha_Inicio { get; set; } = DateTime.Now;
        public DateTime Fecha_Fin { get; set; } = DateTime.Now;
    }
}
