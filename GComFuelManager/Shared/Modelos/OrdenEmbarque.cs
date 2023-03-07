using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenEmbarque
    {
        [JsonProperty("cod"), Key] public int Cod { get; set; }
        [JsonProperty("fchOrd")] public DateTime? FchOrd { get; set; }
        [JsonProperty("fchPro")] public DateTime? FchPro { get; set; }
        [JsonProperty("codtad"), Required(ErrorMessage = "El campo de terminal es requerido.")] public Int16? Codtad { get; set; }
        [JsonProperty("codprd"), Required(ErrorMessage = "El campo de producto es requerido.")] public byte? Codprd { get; set; }
        [JsonProperty("vol"), Required(ErrorMessage = "El campo de cantidad es requerido.")] public double? Vol { get; set; }
        [JsonProperty("codchf")] public int? Codchf { get; set; }
        [JsonProperty("coddes"), Required(ErrorMessage = "El campo de estacion es requerido.")] public int? Coddes { get; set; }
        [JsonProperty("codest")] public byte? Codest { get; set; }
        [JsonProperty("fchpet")] public DateTime? Fchpet { get; set; }
        [JsonProperty("fchcar"), Required(ErrorMessage = "El campo de fecha de carga es requerido.")] public DateTime? Fchcar { get; set; }
        [JsonProperty("codton")] public int? Codton { get; set; }
        [JsonProperty("bin")] public int? Bin { get; set; }
        [JsonProperty("codusu")] public int? Codusu { get; set; }
        [JsonProperty("folio")] public int? Folio { get; set; }
        [JsonProperty("pre"), Required(ErrorMessage = "El campo de precio es requerido.")] public double? Pre { get; set; }
        [JsonProperty("codordCom")] public int? CodordCom { get; set; }
        [JsonProperty("bolguidid")] public string? Bolguidid { get; set; }
        [JsonProperty("tp")] public bool? Tp { get; set; }
        [JsonProperty("CompartmentId")] public int? CompartmentId { get; set; }
        [JsonProperty("compartment")] public int? Compartment { get; set; }
        [JsonProperty("numTonel")] public int? NumTonel { get; set; }


        [NotMapped] public Destino? Destino { get; set; } = null!;
        [NotMapped] public Tad? Tad { get; set; }
        [NotMapped] public Producto? Producto { get; set; }
        [NotMapped] public Tonel? Tonel { get; set; }
    }
}
