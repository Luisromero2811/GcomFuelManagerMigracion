using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class PreciosDTO
    {
        [DisplayName("Zona")]
        public string Zona { get; set; } = string.Empty;
        [DisplayName("Cliente")]
        public string Cliente { get; set; } = string.Empty;
        [DisplayName("Fecha")]
        public string Fecha { get; set; } = string.Empty;
        [DisplayName("Precio")]
        public double Precio { get; set; } = 0;
        [DisplayName("Producto")]
        public string Producto { get; set; } = string.Empty;
        [DisplayName("Destino")]
        public string Destino { get; set; } = string.Empty;
        [DisplayName("Codigo Synthesis")]
        public string CodSyn { get; set; } = string.Empty;
        [DisplayName("Codigo Tuxpan")]
        public string CodTux { get; set; } = string.Empty;

    }
}
