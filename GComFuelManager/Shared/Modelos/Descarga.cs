using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json; 
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GComFuelManager.Shared.Modelos
{
    //5
    [Table("Descargas")]
	public class Descarga
	{
		[JsonProperty("idDescarga"), Key]
		public int idDescarga { get; set; }

		[JsonProperty("fecha_carga")]
		public DateTime? fecha_carga { get; set; } = DateTime.MinValue;

		[JsonProperty("fecha_descarga")]
		public DateTime? fecha_descarga { get; set; } = DateTime.MinValue;

		[JsonProperty("cliente"), MaxLength(100)]
		public string? cliente { get; set; } = string.Empty;

		[JsonProperty("transportista"), MaxLength(255)]
		public string? transportista { get; set; } = string.Empty;

		[JsonProperty("operador"), MaxLength(100)]
		public string? operador { get; set; } = string.Empty;

		[JsonProperty("tracto"), MaxLength(50)]
		public string? tracto { get; set; } = string.Empty;

        [JsonProperty("placa_tracto"), MaxLength(50)]
        public string? placa_tracto { get; set; } = string.Empty;

        [JsonProperty("placa_autotanque"), MaxLength(50)]
        public string? placa_autotanque { get; set; } = string.Empty;

        [JsonProperty("producto"), MaxLength(100)]
        public string? producto { get; set; } = string.Empty;

        [JsonProperty("tipo")]
        public int? tipo { get; set; } = 0;

        [JsonProperty("terminal_origen"), MaxLength(100)]
        public string? terminal_origen { get; set; } = string.Empty;

        [JsonProperty("destino"), MaxLength(100)]
        public string? destino { get; set; } = string.Empty;

        [JsonProperty("factura"), MaxLength(50)]
        public string? factura { get; set; } = string.Empty;

        [JsonProperty("bol"), MaxLength(10)]
        public string? bol { get; set; } = string.Empty;

        [JsonProperty("sellos"), MaxLength(50)]
        public string? sellos { get; set; } = string.Empty;

        [JsonProperty("BOL_volN"), MaxLength(20)]
        public string? BOL_volN { get; set; } = string.Empty;

        [JsonProperty("BOL_vol20"), MaxLength(20)]
        public string? BOL_vol20 { get; set; } = string.Empty;

        [JsonProperty("fac_volN"), MaxLength(20)]
        public string? fac_volN { get; set; } = string.Empty;

        [JsonProperty("fac_vol20"), MaxLength(20)]
        public string? fac_vol20 { get; set; } = string.Empty;

        [JsonProperty("tnq_volN"), MaxLength(20)]
        public string? tnq_volN { get; set; } = string.Empty;

        [JsonProperty("tnq_vol20"), MaxLength(20)]
        public string? tnq_vol20 { get; set; } = string.Empty;

        [JsonProperty("observaciones")]
        public string? observaciones { get; set; } = string.Empty;

        [JsonProperty("foto_1"), MaxLength(255)]
        public string? foto_1 { get; set; } = string.Empty;

        [JsonProperty("sellos_coincide")]
        public int? sellos_coincide { get; set; } = 0;

        [JsonProperty("venta"), MaxLength(10)]
        public string? venta { get; set; } = string.Empty;

        [JsonProperty("diferencia"), MaxLength(10)]
        public string? diferencia { get; set; } = string.Empty;

        [JsonProperty("doc"), MaxLength(100)]
        public string? doc { get; set; } = string.Empty;

        [JsonProperty("fto"), MaxLength(150)]
        public string? fto { get; set; } = string.Empty;

        [JsonProperty("fecha_creacion")]
        public DateTime? fecha_creacion { get; set; } = DateTime.MinValue;
    }
}

