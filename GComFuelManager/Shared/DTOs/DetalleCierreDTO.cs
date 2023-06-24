using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class DetalleCierreDTO
    {
        [DisplayName("Folio")] public string? OC { get;set; } = string.Empty;
        [DisplayName("BOL")] public string? BOL { get; set; } = string.Empty;
        [DisplayName("Folio de referencia")] public string? FolioReferencia { get; set; } = string.Empty;
        [DisplayName("Fecha de cierre")] public string? FchCierre { get; set; } = string.Empty;
        [DisplayName("Destino")] public string? Destino { get; set; } = string.Empty;
        [DisplayName("Producto")] public string? Producto { get; set; } = string.Empty;
        [DisplayName("Precio")] public string? Precio { get; set; } = string.Empty;
        [DisplayName("Volumen")] public string? Volumen { get; set; } = string.Empty;
        [DisplayName("Unidad")] public string? Unidad { get; set; } = string.Empty;
        [DisplayName("Estatus")] public string? Estatus { get; set; } = string.Empty;
        [DisplayName("Fecha de llegada")] public string? FchLlegada { get; set; } = string.Empty;
        [DisplayName("Observaciones")] public string? Observaciones { get; set; } = string.Empty;
    }
}
