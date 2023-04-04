using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenCierre
    {
        [Key, JsonPropertyName("cod"), EpplusIgnore]
        public int Cod { get; set; }
        
        [JsonPropertyName("fchCierre"),EpplusIgnore]
        public DateTime? FchCierre { get; set; } = DateTime.Now;
        
        [DisplayName("Fecha de cierre"), NotMapped]
        public string? fch { get { return FchCierre!.Value.ToString("dd/MM/yyyy"); } }
        
        [JsonPropertyName("folio"), DisplayName("Folio")]
        public string? Folio { get; set; } = string.Empty;
        
        [JsonPropertyName("contacto"), DisplayName("Contacto")]
        public string? Contacto { get; set; } = string.Empty;
        
        [JsonPropertyName("email"), DisplayName("Correo")]
        public string? Email { get; set; } = string.Empty;
        
        [JsonPropertyName("codPrd"), EpplusIgnore]
        public byte? CodPrd { get; set; }
        
        [NotMapped, EpplusIgnore] 
        public Producto? Producto { get; set; } = null!;
        
        [JsonPropertyName("codCte"), EpplusIgnore]
        public int? CodCte { get; set; }
        
        [NotMapped, EpplusIgnore] 
        public Cliente? Cliente { get; set; } = null!;
        
        [JsonPropertyName("tipoVenta"), DisplayName("Tipo de Venta")]
        public string? TipoVenta { get; set; } = string.Empty;
        
        [JsonPropertyName("precio"), DisplayName("Precio")]
        public double? Precio { get; set; }
        
        [JsonPropertyName("temperatura"), DisplayName("Temperatura")]
        public double? Temperatura { get; set; }
        
        [JsonPropertyName("vendedor"), DisplayName("Vendedor")]
        public string? Vendedor { get; set; } = string.Empty;
        
        [JsonPropertyName("codDes"), EpplusIgnore]
        public int? CodDes { get; set; }
        
        [NotMapped, EpplusIgnore] 
        public Destino? Destino { get; set; } = null!;
        
        [JsonPropertyName("volumen"), DisplayName("Volumen")]
        public int? Volumen { get; set; }
        
        [JsonPropertyName("observaciones"), DisplayName("Observaciones")]
        public string? Observaciones { get; set; } = string.Empty;
        
        [JsonPropertyName("estatus"), EpplusIgnore]
        public bool? Estatus { get; set; } = true;

        [JsonPropertyName("confirmada"), EpplusIgnore]
        public bool? Confirmada { get; set; } = false;
    }
}
