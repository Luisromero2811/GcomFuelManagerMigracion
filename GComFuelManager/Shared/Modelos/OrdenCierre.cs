using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenCierre
    {
        [Key,JsonPropertyName("cod")]
        public int Cod { get; set; }
        [JsonPropertyName("fchCierre")]
        public DateTime FchCierre { get; set; } = DateTime.Now;
        [JsonPropertyName("folio")]
        public string Folio { get; set; } = string.Empty;
        [JsonPropertyName("contacto")]
        public string Contacto { get; set; } = string.Empty;
        [JsonPropertyName("email")]
        public string Email { get; set;} = string.Empty;
        [JsonPropertyName("codPrd")]
        public byte CodPrd { get; set; }
        [NotMapped] public Producto? Producto { get; set; } = null!;
        [JsonPropertyName("codCte")]
        public int CodCte { get; set; }
        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [JsonPropertyName("tipoVenta")]
        public string TipoVenta { get; set; } = string.Empty;
        [JsonPropertyName("precio")]
        public double Precio { get; set; }
        [JsonPropertyName("temperatura")]
        public double Temperatura { get; set; }
        [JsonPropertyName("vendedor")]
        public string Vendedor { get; set; } = string.Empty;
        [JsonPropertyName("codDes")]
        public int CodDes { get; set; }
        [NotMapped] public Destino? Destino { get; set; } = null!;
        [JsonPropertyName("volumen")]
        public int Volumen { get; set; }
        [JsonPropertyName("observaciones")]
        public string Observaciones { get; set; } = string.Empty;
        [JsonPropertyName("estatus")]
        public bool Estatus { get; set; } = true;
        [JsonPropertyName("confirmada")]
        public bool Confirmada { get; set; } = false;
    }
}
