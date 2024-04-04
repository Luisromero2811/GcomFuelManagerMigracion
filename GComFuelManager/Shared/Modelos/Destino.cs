using OfficeOpenXml.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class Destino
    {
        [Key]
        public int Cod { get; set; }
        public int? Num { get; set; } = 0;
        [MaxLength(128)]
        public string? Den { get; set; } = string.Empty;
        public int? Codcte { get; set; } = 0;
        [MaxLength(30)]
        public string? Nroper { get; set; } = string.Empty;
        public string? Dir { get; set; } = string.Empty;
        public int? CodbdTan { get; set; } = 0;
        [MaxLength(10)]
        public string? DesCod { get; set; } = string.Empty;
        [MaxLength(20)]
        public string? Codsyn { get; set; } = string.Empty;
        public bool? Esenergas { get; set; } = false;
        public bool Activo { get; set; } = true;
        [MaxLength(50)]
        public string? Lat { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Lon { get; set; } = string.Empty;
        public Int16? Codciu { get; set; } = 0;
        [MaxLength(50)]
        public string? Ciu { get; set; } = string.Empty;
        [MaxLength(50)]
        public string? Est { get; set; } = string.Empty;
        public long? CodGamo { get; set; } = 0;
        public short? Id_Tad { get; set; }

        public string? Id_DestinoGobierno { get; set; } = string.Empty;

        [EpplusIgnore, NotMapped] public List<Tad> Terminales { get; set; } = new();
        [EpplusIgnore, NotMapped, JsonIgnore] public List<Destino_Tad> Destino_Tads { get; set; } = new();
        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public OrdenCierre? OrdenCierre { get; set; } = null!;
        [NotMapped] public Producto? Producto { get; set; } = null!;
        [NotMapped] public Tad? Tad { get; set; } = null!;

        public Destino HardCopy()
        {
            return new()
            {
                Cod = Cod,
                Num = Num,
                Den = Den,
                Codcte = Codcte,
                Nroper = Nroper,
                Dir = Dir,
                CodbdTan = CodbdTan,
                DesCod = DesCod,
                Codsyn = Codsyn,
                Esenergas = Esenergas,
                Activo = Activo,
                Lat = Lat,
                Lon = Lon,
                Codciu = Codciu,
                Ciu = Ciu,
                Est = Est,
                CodGamo = CodGamo,
                Id_DestinoGobierno = Id_DestinoGobierno
            };
        }
    }
}
