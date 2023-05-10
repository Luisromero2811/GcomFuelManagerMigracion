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
        [JsonProperty("zona"),DisplayName("Zona")]
        public string Zona { get; set; } = string.Empty;
        [JsonProperty("cod"), EpplusIgnore]
        public int Cod { get; set; } = 0;
        [JsonProperty("codCte"), DisplayName("Identificador Cliente")]
        public int CodCte { get; set; } = 0;
        [JsonProperty("codPrd"), EpplusIgnore]
        public int CodPrd { get; set; } = 0;
        [JsonProperty("cliente"), DisplayName("Cliente")]
        public string Cliente { get; set; } = string.Empty;
        [JsonProperty("fecha"), DisplayName("Fecha")]
        public string Fecha { get; set; } = string.Empty;
        [JsonProperty("precio"), DisplayName("Precio")]
        public double Precio { get; set; } = 0;
        [JsonProperty("codZona"), EpplusIgnore]
        public int CodZona { get; set; } = 0;

    }
}
