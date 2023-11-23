using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class FolioDetalleDTO
    {
        [EpplusIgnore]
        public DateTime? FchCierre { get; set; } = null!;
        [EpplusIgnore]
        public Cliente? Cliente { get; set; } = null!;
        [EpplusIgnore]
        public Destino? Destino { get; set; } = null!;
        [EpplusIgnore]
        public Producto? Producto { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public string? Estado { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public OrdenEmbarque? ordenEmbarque { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public OrdenCierre? ordenCierre { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public bool? Activa { get; set; } = true;
        [NotMapped, EpplusIgnore]
        public Grupo? Grupo { get; set; } = null!;

        [DisplayName("Fecha de cierre")]
        public string FchCie { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:d}", FchCierre); } }
        [DisplayName("Folio")]
        public string? Folio { get; set; } = string.Empty;
        [DisplayName("Grupo")]
        public string NGrupo { get { return Grupo != null ? Grupo.Den! : string.Empty; } }
        [DisplayName("Cliente")]
        public string NCliente { get { return Cliente != null ? Cliente.Den! : string.Empty; } }
        [DisplayName("Destino")]
        public string NDestino { get { return Destino != null ? Destino.Den! : string.Empty; } }
        [DisplayName("Producto")]
        public string NProducto { get { return Producto != null ? Producto.Den! : string.Empty; } }
        [DisplayName("Precio")]
        public double? Precio { get; set; } = 0;
        [DisplayName("Comentarios")]
        public string? Comentarios { get; set; } = null!;
    }
}
