using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Transportista
    {
        [Key, EpplusIgnore]
        public int Cod { get; set; } = 0;
        [MaxLength(256), DisplayName("Nombre de Transportista")]
        public string? Den { get; set; } = string.Empty;
        [AllowNull, DefaultValue(""), EpplusIgnore]
        public string? CarrId { get; set; } = string.Empty;
        [MaxLength(15), EpplusIgnore]
        public string? Busentid { get; set; } = string.Empty;
        [EpplusIgnore] public bool? Activo { get; set; } = true;
        [EpplusIgnore] public bool? Simsa { get; set; } = true;
        [EpplusIgnore] public string? Gru { get; set; } = string.Empty;

        //Union con GrupoTransportista
        [NotMapped, EpplusIgnore]
        public GrupoTransportista? GrupoTransportista { get; set; } = null!;

        [EpplusIgnore] public int? Codgru { get; set; } = null!;
        [EpplusIgnore] public short? Id_Tad { get; set; }

        [EpplusIgnore, NotMapped] public List<Tad> Terminales { get; set; } = new();
        [EpplusIgnore, NotMapped, JsonIgnore] public List<Transportista_Tad> Transportista_Tads { get; set; } = new();

        [NotMapped, EpplusIgnore] public Tad? Tad { get; set; } = null!;
    }
}

