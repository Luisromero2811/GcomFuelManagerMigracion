using System;
using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs
{
	public class FormatoExcelPrecioDTO
	{
        [DisplayName("PRODUCTO")]
        public string? Producto { get; set; } = string.Empty;
        [DisplayName("ZONA")]
        public string? Zona { get; set; } = string.Empty;
        [DisplayName("CLIENTE")]
        public string? Cliente { get; set; } = string.Empty;
        [DisplayName("DESTINO")]
        public string? Destino { get; set; } = string.Empty;
        [DisplayName("CODIGO SYNTHESIS")]
        public string? CodSyn { get; set; } = string.Empty;
        [DisplayName("CODIGO TUXPAN")]
        public string? CodTux { get; set; } = string.Empty;
        [DisplayName("CODIGO GOBIERNO")]
        public string? CodDestinoGobierno { get; set; } = string.Empty;
        [DisplayName("FECHA")]
        public string? Fecha { get; set; } = string.Empty;
        [DisplayName("PRECIO FINAL")]
        public double Precio { get; set; } = 0;
        [DisplayName("MONEDA")]
        public string Moneda { get; set; } = string.Empty;
        [DisplayName("TIPO DE CAMBIO")]
        public double TipoCambio { get; set; } = 0;
    }
}

