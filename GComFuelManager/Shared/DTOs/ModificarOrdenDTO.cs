using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class ModificarOrdenDTO
    {
        public int? Id_Orden { get; set; }
        public string? Referencia { get; set; } = string.Empty;
        public int? Bin { get; set; }
        public string? Cliente { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;
        public double? Volumen { get; set; }
        public double? Precio { get; set; }
        public DateTime? Fecha_Peticion { get; set; }
        public DateTime? Fecha_Programa { get; set; }
        public string? Transportista { get; set; } = string.Empty;
        public string? Chofer { get; set; } = string.Empty;
        public string? Unidad { get; set; } = string.Empty;
        public int? Compartimiento { get; set; }
        public string? Estado { get; set; } = string.Empty;
        public string? Url_PDF_Facturacion { get; set; } = string.Empty;
        public string? Url_XML_Facturacion { get; set; } = string.Empty;
        public string? Url_Archivo_BOL { get; set; } = string.Empty;

        public bool Mostrar_Detalle { get; set; } = false;
        public List<ModificarOrdenCargadaDTO> OrdenesCargadas { get; set; } = new();
    }
}
