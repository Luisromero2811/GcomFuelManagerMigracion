using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenEmbarque
    {
        [JsonProperty("cod"), Key] public int Cod { get; set; }
        [JsonProperty("fchOrd")] public DateTime? FchOrd { get; set; }
        [JsonProperty("fchPro")] public DateTime? FchPro { get; set; }
        [JsonProperty("codtad")] public Int16? Codtad { get; set; } = 1;
        [JsonProperty("codprd")] public byte? Codprd { get; set; } = 0;
        [JsonProperty("vol"), EpplusIgnore] public double? Vol { get; set; } = 0;
        [DisplayName("Volumen")]
        public string Volumenes { get { return Vol.Value.ToString("N2"); } }
        [JsonProperty("codchf")] public int? Codchf { get; set; }
        [JsonProperty("coddes")] public int? Coddes { get; set; } = 0;
        [JsonProperty("codest")] public byte? Codest { get; set; }
        [JsonProperty("fchpet")] public DateTime? Fchpet { get; set; }
        [JsonProperty("fchcar")] public DateTime? Fchcar { get; set; } = DateTime.Today;
        [JsonProperty("codton")] public int? Codton { get; set; }
        [JsonProperty("bin")] public int? Bin { get; set; }
        [JsonProperty("codusu")] public int? Codusu { get; set; }
        [JsonProperty("folio")] public int? Folio { get; set; }
        [JsonProperty("pre")] public double? Pre { get; set; } = 0;
        [JsonProperty("codordCom")] public int? CodordCom { get; set; }
        [JsonProperty("bolguidid")] public string? Bolguidid { get; set; }
        [JsonProperty("tp")] public bool? Tp { get; set; }
        [JsonProperty("CompartmentId")] public int? CompartmentId { get; set; }
        [JsonProperty("compartment")] public int? Compartment { get; set; } = 1;
        [JsonProperty("numTonel")] public int? NumTonel { get; set; }

        [NotMapped] public Destino? Destino { get; set; } = null!;
        [NotMapped] public Tad? Tad { get; set; } = null!;
        [NotMapped] public Producto? Producto { get; set; } = null!;
        [NotMapped] public Tonel? Tonel { get; set; } = null!;
        [NotMapped] public Chofer? Chofer { get; set; } = null!;

        [NotMapped] public OrdenCompra? OrdenCompra { get; set; } = null!;
        [NotMapped] public Estado? Estado { get; set; } = null!;

        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public Usuario? Usuario { get; set; } = null!;

        [NotMapped] public Orden? Orden { get; set; } = null!;

        [NotMapped] public Transportista? Transportista { get; set; } = null!;

        [NotMapped] public OrdenCierre? OrdenCierre { get; set; } = null!;
    }
}
