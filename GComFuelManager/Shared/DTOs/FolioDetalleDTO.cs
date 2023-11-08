using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class FolioDetalleDTO
    {
        public string? Folio { get; set; } = string.Empty;
        public Cliente? Cliente { get; set; } = null!;
        public Destino? Destino { get; set; } = null!;
        public Producto? Producto { get; set; } = null!;
        public DateTime? FchCierre { get; set; } = null!;
        public string? Comentarios { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public string? Estado { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public OrdenEmbarque? ordenEmbarque { get; set; } = null!;
        [NotMapped, EpplusIgnore]
        public bool? Activa { get; set; } = true;
        [NotMapped, EpplusIgnore]
        public Grupo? Grupo { get; set; } = null!;
    }
}
