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
        public string? Folio { get; set; } = string.Empty;

        [EpplusIgnore]
        public OrdenEmbarque? OrdenEmbarque { get; set; } = null!;
        [DisplayName("BOL/Embarque")]
        public int? BOL { get; set; } = 0;

        [EpplusIgnore]
        public DateTime? FchCierre { get; set; } = null!;
        [DisplayName("Fecha de la Orden")]
        public string? FechaCierre { get; set; } = string.Empty;

        [EpplusIgnore]
        public Cliente? Cliente { get; set; } = null!;

        [EpplusIgnore]
        public Destino? Destino { get; set; } = null!;
        [DisplayName("Nombre del Destino")]
        public string? NombreDestino { get; set; } = string.Empty!;

        [EpplusIgnore]
        public Producto? Producto { get; set; } = null!;
        [DisplayName("Nombre del Producto")]
        public string? NombreProducto { get; set; } = string.Empty!;

        [EpplusIgnore]
        public string? Comentarios { get; set; } = null!;

        [EpplusIgnore]
        public Estado? Estado { get; set; } = null!;
        [DisplayName("Estado de la Orden")]
        public string? NombreEstado { get; set; } = string.Empty!;

    }
}
