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
    public class Orden
    {
        [JsonProperty("cod"), Key]
        public Int64? Cod { get; set; }
        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;
        [JsonProperty("ref"), MaxLength(32)]
        public string? Ref { get; set; } = string.Empty;
        [JsonProperty("coddes")]
        public int? Coddes { get; set; } = 0;
        //
        [JsonProperty("codprd"), Column(TypeName = "tinyint")]
        public int? Codprd { get; set; } = Convert.ToByte(0); //PK

        [JsonProperty("vol")]
        public float? Vol { get; set; } = float.NaN;
        [JsonProperty("fchcar")]
        public DateTime? Fchcar { get; set; } = DateTime.MinValue;
        //Prueba
        [JsonProperty("codest"), Column(TypeName = "tinyint")]
        public Int16? Codest { get; set; } = Convert.ToByte(0); //PK

        [JsonProperty("coduni")]
        public int? Coduni { get; set; } = 0;
        [JsonProperty("codchf")]
        public int? Codchf { get; set; } = 0;
        [JsonProperty("bolguiid"),MaxLength(256)]
        public string? Bolguiid { get; set; } = string.Empty;
        [JsonProperty("liniteid")]
        public Int64? Liniteid { get; set; }
        [JsonProperty("codprd2")]
        public int? Codprd2 { get; set; } = 0;
        [JsonProperty("dendes"), MaxLength(256)]
        public string? Dendes { get; set; } = string.Empty;
        [JsonProperty("vol2")]
        public float? Vol2 { get; set; } = float.NaN;
        [JsonProperty("batchId")]
        public Int64? BatchId { get; set; }
        [JsonProperty("CompartmentId")]
        public int? CompartmentId { get; set; }
        [JsonProperty("SealNumber"), MaxLength(128)]
        public string? SealNumber { get; set; } = string.Empty;

        //Prop de nav Estado
        [NotMapped] public Estado? Estado { get; set; } = null!;
        [NotMapped] public Destino? Destino { get; set; } = null!;
        [NotMapped] public Producto? Producto { get; set; }

        [NotMapped] public Tonel? Tonel { get; set; } = null!;
        [NotMapped] public Chofer? Chofer { get; set; } = null!;

        [NotMapped] public OrdenCompra? OrdenCompra { get; set; } = null!;
   

        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public Usuario? Usuario { get; set; } = null!;

        [NotMapped] public Transportista? Transportista { get; set; } = null!;
    }
}
