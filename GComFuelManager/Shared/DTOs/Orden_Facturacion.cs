using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class Orden_Facturacion : Parametros_Busqueda_Gen
    {
        //public long? Id_Orden { get; set; }
        public string? Terminal { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        [DisplayName("Tipo de venta")]
        public string? Tipo_Venta { get; set; } = string.Empty;
        public string? TipoCompra { get; set; } = string.Empty;
        public string? Cliente { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;
        [DisplayName("Volumen cargado")]
        public double? Volumen_Cargado { get; set; }
        [DisplayName("Importe de Compra")]
        public string? Importe_Compra { get; set; }
        [DisplayName("Precio Compra s/impuesto")]
        public double? Precio_CompraSImpuesto { get; set; }
        [DisplayName("BOL/Emarque")]
        public string? BOL { get; set; } = string.Empty;
        public string? Factura_Proveeedor { get; set; } = string.Empty;
        [EpplusIgnore]
        public DateTime? Fecha_Carga { get; set; }
        [DisplayName("Fecha de carga")]
        public string? Fecha_Carga_Formato { get => Fecha_Carga?.ToString("d"); }
        public string? Tranportista { get; set; } = string.Empty;
        public string? Chofer { get; set; } = string.Empty;
        public string? Unidad { get; set; } = string.Empty;
        public string? Cliente { get; set; } = string.Empty;
        public double? Precio { get; set; }
        [DisplayName("Numero de orden")]
        public string? No_Orden { get; set; } = string.Empty;
        public string? Sellos { get; set; } = string.Empty;
        public string? Pedimento { get; set; } = string.Empty;
        [EpplusIgnore]
        public List<Archivo> Archivos { get; set; } = new();
        //public string? Referencia { get; set; } = string.Empty;
        //public string? Bolguid { get; set; } = string.Empty;
        //public int? CompartimientoId { get; set; }
        //public short? Id_Terminal { get; set; }
    }
}
