using System.ComponentModel.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using OfficeOpenXml.Attributes;
using System.ComponentModel;

namespace GComFuelManager.Shared.Modelos
{
    public class Chofer
    {
        [Key, EpplusIgnore]
        public int Cod { get; set; }
        [MaxLength(128), DisplayName("Nombre del Chofer")]
        public string? Den { get; set; } = string.Empty;
        [EpplusIgnore]public int? Codtransport { get; set; } = 0;
        [MaxLength(6), EpplusIgnore]
        public string? Dricod { get; set; } = string.Empty;
        [MaxLength(128), DisplayName("Apellidos del Chofer")]
        public string? Shortden { get; set; } = string.Empty;
        [EpplusIgnore] public bool? Activo { get; set; } = true;
        [EpplusIgnore] public bool? Activo_Permanente { get; set; } = true;
        [EpplusIgnore] public short? Id_Tad { get; set; }
        [MaxLength(50)]
        public string? RFC { get; set; } = string.Empty;
        [NotMapped, EpplusIgnore] public int? CodTra { get; set; }
        [NotMapped, DisplayName("Nombre completo del Chofer")]
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(Den) && !string.IsNullOrEmpty(Shortden) && Den.ToLower().Equals(Shortden.ToLower()))
                    return Shortden;
                else
                    return $"{Den} {Shortden}";
            }
        }

        [NotMapped, EpplusIgnore] public Transportista? Transportista { get; set; } = null!;
        [NotMapped, EpplusIgnore] public Tonel? Tonel { get; set; } = null!;
        [NotMapped, EpplusIgnore] public List<Tad> Terminales { get; set; } = new();
        [NotMapped, JsonIgnore, EpplusIgnore] public List<Chofer_Tad> Chofer_Tads { get; set; } = new();
    }
}

