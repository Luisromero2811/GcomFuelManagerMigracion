using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdEmbDet
    {
        [JsonProperty("cod"), Key]
        public int Cod { get; set; }
        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("fchDoc")]
        public DateTime? FchDoc { get; set; } = DateTime.MinValue;
        [JsonProperty("eta"), MaxLength(4)] 
        public string? Eta { get; set; } = string.Empty;
        [JsonProperty("fchlleest")]
        public DateTime? Fchlleest { get; set; } = DateTime.MinValue;
        [JsonProperty("fchrealledes")]
        public DateTime? Fchrealledes { get; set; } = DateTime.MinValue;
        [JsonProperty("litent")]
        public decimal? Litent { get; set; } = decimal.MaxValue;
        [JsonProperty("obs"), MaxLength(256)]
        public string? Obs { get; set; } = string.Empty;
        [JsonProperty("bol")]
        public int? Bol { get; set; } = 0;
        [JsonProperty("codusu")]
        public int Codusu { get; set; }
        [JsonProperty("fchmod")]
        public DateTime Fchmod { get; set; } = DateTime.MinValue;
        [JsonProperty("codusumod")]
        public int Codusumod { get; set; }
        [JsonProperty("loc")]
        public string? Loc { get; set; } = string.Empty;

        //Propiedad de navegación
        [NotMapped] public Orden Orden { get; set; } = null!;
    }
}
