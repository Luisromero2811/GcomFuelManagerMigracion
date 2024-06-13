using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class Reporte_Cargas_Tad_SJI
    {
        [DisplayName("SoldTo No.")]
        public string SoldTo { get; set; } = "ENE01";
        [DisplayName("Cust. Sold-to Name")]
        public string CustSoldToName { get; set; } = "ENERGAS DE MEXICO";
        [DisplayName("ShipTo Name")]
        public string ShipToName { get; set; } = string.Empty;
    }
}
