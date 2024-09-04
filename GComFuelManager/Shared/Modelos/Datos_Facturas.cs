using System.ComponentModel.DataAnnotations;

namespace GComFuelManager.Shared.Modelos
{
    public class Datos_Facturas
    {
        public int Id { get; set; }
        public int Id_Orden { get; set; }
        [StringLength(15)]
        public string Numero_Orden { get; set; } = string.Empty;
        public DateTime? Fecha_Numero_Orden { get; set; } = null!;
        [StringLength(20)]
        public string Factura_MGC { get; set; } = string.Empty;
        public DateTime? Fecha_Factura_MGC { get; set; } = null!;
        [StringLength(20)]
        public string Factura_MexicoS { get; set; } = string.Empty;
        public DateTime? Fecha_Factura_MexicoS { get; set; } = null!;
        [StringLength(15)]
        public string Factura_DCL { get; set; } = string.Empty;
        public DateTime? Fecha_Factura_DCL { get; set; } = null!;
        [StringLength(20)]
        public string Factura_Energas { get; set; } = string.Empty;
        public DateTime? Fecha_Factura_Energas { get; set; } = null!;

        public OrdenEmbarque? OrdenEmbarque { get; set; } = null!;
    }
}
