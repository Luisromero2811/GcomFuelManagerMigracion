using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GComFuelManager.Shared.DTOs
{
    public class ListadoFolioDetalle
    {
        public string? Cliente { get; set; } = string.Empty;
        public string? Folio { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public double? Volumen_Disponible { get; set; } = 0;
        public int? Ordenes_Relacionadas { get; set; } = 0;
        public string? Comentarios { get; set; } = string.Empty;
        public DateTime? Fecha_Cierre { get; set; } = DateTime.Today;
    }
}
