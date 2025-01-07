using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Destino
    {
        [Key]
        public int Cod { get; set; }
        public int? Num { get; set; } = 0;
        public string? Den { get; set; } = string.Empty;
        public int? Codcte { get; set; } = 0;
        public string? Nroper { get; set; } = string.Empty;
        public string? Dir { get; set; } = string.Empty;
        public int? CodbdTan { get; set; } = 0;
        public string? DesCod { get; set; } = string.Empty;
        public string? Codsyn { get; set; } = string.Empty;
        public bool? Esenergas { get; set; } = false;
        public bool? Activo { get; set; } = true;
        public string? Lat { get; set; } = string.Empty;
        public string? Lon { get; set; } = string.Empty;
        public Int16? Codciu { get; set; } = 0;
        public string? Ciu { get; set; } = string.Empty;
        public string? Est { get; set; } = string.Empty;

        [EpplusIgnore, NotMapped] public List<Tad> Terminales { get; set; } = new();
        [EpplusIgnore] public short? Id_Tad { get; set; }
        [NotMapped] public Cliente? Cliente { get; set; } = null!;

        public override string ToString()
        {
            return Den ?? string.Empty;
        }
    }
}
