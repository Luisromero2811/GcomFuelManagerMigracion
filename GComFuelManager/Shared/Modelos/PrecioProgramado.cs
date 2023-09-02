using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class PrecioProgramado
    {
        [Key, JsonProperty("cod"), EpplusIgnore] public int? Cod { get; set; }
        [JsonProperty("codZona"), EpplusIgnore] public int? codZona { get; set; } = 0;
        [JsonProperty("codDes"), EpplusIgnore] public int? codDes { get; set; } = 0;
        [JsonProperty("codCte"), EpplusIgnore] public int? codCte { get; set; } = 0;
        [JsonProperty("codGru"), EpplusIgnore] public Int16? codGru { get; set; } = 0;
        [JsonProperty("codPrd"), EpplusIgnore] public byte? codPrd { get; set; } = 0;
        [JsonProperty("pre")] public double Pre { get; set; } = 0;
        [JsonProperty("fchActualizacion"), EpplusIgnore] public DateTime FchActualizacion { get; set; } = DateTime.Now;
        [JsonProperty("fchDia"), EpplusIgnore] public DateTime FchDia { get; set; } = DateTime.Now;
        [JsonProperty("Activo"), EpplusIgnore] public bool Activo { get; set; } = true;
        [NotMapped, EpplusIgnore] public Zona? Zona { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Cliente? Cliente { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Producto? Producto { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Destino? Destino { get; set; } = null!;
    }
}
