using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class ModificarOrdenCargadaDTO
    {
        public long? Id_Orden { get; set; }
        public string? Referencia { get; set; } = string.Empty;
        public int? Bol { get; set; }
        public string? Cliente { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;
        public double? Volumen { get; set; }
        public DateTime? Fecha_Carga { get; set; }
        public DateTime? Fecha_Llegada { get; set; }
        public string? Transportista { get; set; } = string.Empty;
        public string? Chofer { get; set; } = string.Empty;
        public string? Unidad { get; set; } = string.Empty;
        public string? Estado { get; set; } = string.Empty;
        public string? Pedimento { get; set; } = string.Empty;
        public string? Sellos { get; set; } = string.Empty;
        public string? NOrden { get; set; } = string.Empty;
        public string? Factura { get; set; } = string.Empty;
        public long? Id_Bol { get; set; }
        public int Id_Cliente { get; set; }
        public int Id_Destino { get; set; }
        public short? Id_Grupo { get; set; }
        public int Id_Tonel { get; set; }
        public int Id_Transportista { get; set; }
        public int Id_Chofer { get;set; }
        public byte Id_Producto { get; set; }
        public double? Precio { get; set; }
        //public ModificarOrdenDTO ModificarOrdenDTO { get; set; } = null!;
    }
}
