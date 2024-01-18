using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class Cliente
    {
        [JsonProperty("cod"), Key, EpplusIgnore]
        public int Cod { get; set; }
        [JsonProperty("den"), MaxLength(128), DisplayName("Nombre del cliente")]
        public string? Den { get; set; } = string.Empty;
        [JsonProperty("codusu"), EpplusIgnore]
        public int? Codusu { get; set; } = 0;
        [JsonProperty("codforpag"), EpplusIgnore]
        public int? Codforpag { get; set; } = 0;
        [JsonProperty("tem"), MaxLength(50), EpplusIgnore]
        public string? Tem { get; set; } = string.Empty;
        [JsonProperty("codgru"), EpplusIgnore]
        public Int16? codgru { get; set; } = null!;

        [JsonProperty("email"), MaxLength(30), EpplusIgnore]
        public string? Email { get; set; } = string.Empty;
        [JsonProperty("con"), MaxLength(50), EpplusIgnore]
        public string? Con { get; set; } = string.Empty;
        [JsonProperty("codtad"), EpplusIgnore]
        public Int16? Codtad { get; set; } = 0;
        [JsonProperty("codsyn"), MaxLength(20), EpplusIgnore]
        public string? Codsyn { get; set; } = string.Empty;
        [JsonProperty("esenergas"), EpplusIgnore]
        public bool? Esenergas { get; set; } = false;
        [JsonProperty("tipven"), MaxLength(16), DisplayName("Tipo de Venta")]
        public string? Tipven { get; set; } = string.Empty;
        [JsonProperty("codCte"), EpplusIgnore]
        public string? CodCte { get; set; } = string.Empty;
        [JsonProperty("consecutivo"), EpplusIgnore]
        public int? Consecutivo { get; set; } = 0;
        [JsonProperty("activo"), EpplusIgnore]
        public bool Activo { get; set; } = true;
        [JsonProperty("precioSemanal"), EpplusIgnore]
        public bool? precioSemanal { get; set; } = false;
        [NotMapped, EpplusIgnore] public bool IsEditing { get; set; } = false;
        [NotMapped, EpplusIgnore] public string Nuevo_Codigo { get; set; } = string.Empty;

        [JsonProperty("mdVenta"), DisplayName("Modelo de venta")]
        public string? MdVenta { get; set; } = string.Empty;
        [NotMapped, EpplusIgnore] public Grupo? grupo { get; set; } = null!;
    }
}
