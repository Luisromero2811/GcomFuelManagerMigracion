using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.Modelos
{
    public class Grupo
    {
        [JsonProperty("cod"), Key]
        public Int16 Cod { get; set; }

        [JsonProperty("den"), MaxLength(30)]
        public string? Den { get; set; } = string.Empty;

        [JsonProperty("eje"), MaxLength(50)]
        public string? Eje { get; set; } = string.Empty;

        [JsonProperty("fch")]
        public DateTime? Fch { get; set; } = DateTime.MinValue;

        //[JsonProperty("class")]
        //public int? Clase { get; set; } = 0;

        [JsonProperty("tipven"), MaxLength(16)]
        public string? Tipven { get; set; } = string.Empty;
        public string? MdVenta { get; set; } = string.Empty;
        [EpplusIgnore] public string? CodGru { get; set; } = string.Empty;
        [EpplusIgnore, NotMapped] public bool IsEditing { get; set; } = false;
        [EpplusIgnore, NotMapped] public string Nuevo_Codigo { get; set; } = string.Empty;
        //[NotMapped]
        //public Cliente clientess { get; set; }
        //[NotMapped]
        //public int CodCli { get { return clientess.Cod; } }

        public override string ToString()
        {
            return Den ?? string.Empty;
        }
    }
}

