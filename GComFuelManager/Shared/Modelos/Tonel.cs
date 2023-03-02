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
    public class Tonel
    {
        [JsonProperty("cod"), Key] public int Cod { get; set; }
        [JsonProperty("placa"), MaxLength(30)] public string Placa { get; set; } = string.Empty;

        [JsonProperty("codsyn")] public int Codsyn { get; set; } = 0;

        [JsonProperty("carid"), MaxLength(15)] public string Carid { get; set; } = string.Empty;

        [JsonProperty("nrocom")] public int Nrocom { get; set; } = 0;

        [JsonProperty("idcom")] public int Idcom { get; set; } = 0;
        [JsonProperty("capcom")] public decimal Capcom { get; set; } = decimal.Zero;

        [JsonProperty("nrocom2")] public int Nrocom2 { get; set; } = 0;

        [JsonProperty("idcom2")] public int Idcom2 { get; set; } = 0;

        [JsonProperty("capcom2")] public decimal? Capcom2 { get; set; } = decimal.Zero;

        [JsonProperty("nrocom3")] public int Nrocom3 { get; set; } = 0;

        [JsonProperty("idcom3")] public int Idcom3 { get; set; } = 0;

        [JsonProperty("capcom3")] public decimal? Capcom3 { get; set; } = decimal.Zero;

        [JsonProperty("tracto"), MaxLength(20)] public string Tracto { get; set; } = string.Empty;

        [JsonProperty("placatracto"), MaxLength(20)] public string Placatracto { get; set; } = string.Empty;

        [JsonProperty("activo")] public bool Activo { get; set; } = true;

        [JsonProperty("gps")] public bool Gps { get; set; } = false;

        [JsonProperty("nrocom4")] public int Nrocom4 { get; set; } = 0;

        [JsonProperty("idcom4")] public int Idcom4 { get; set; } = 0;

        [JsonProperty("capcom4")] public int? Capcom4 { get; set; } = 0;

    }
}
