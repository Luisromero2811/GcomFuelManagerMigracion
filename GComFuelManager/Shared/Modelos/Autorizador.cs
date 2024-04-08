using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
	public class Autorizador
	{
        [Key, EpplusIgnore]
        public int Cod { get; set; }
        [MaxLength(128), DisplayName("Nombre del Autorizador")]
        public string? Den { get; set; } = string.Empty;
        [EpplusIgnore] public short? Id_Tad { get; set; }


        [NotMapped, EpplusIgnore] public List<Tad> Terminales { get; set; } = new();
        [NotMapped, JsonIgnore] public List<Autorizadores_Tad> Autorizador_Tad { get; set; } = new();
        [JsonIgnore, NotMapped] public Tad? Terminal { get; set; } = null!;
    }
}

