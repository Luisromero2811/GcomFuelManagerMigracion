using System;
using GComFuelManager.Shared.Modelos;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GComFuelManager.Shared.DTOs
{
	public class FolioCierreDTO
	{
		public string? Folio { get; set; } = null!;
		[EpplusIgnore]
		public Cliente? cliente { get; set; } = null!;
		[EpplusIgnore]
		public Destino? destino { get; set; } = null!;
		[EpplusIgnore]
		public Producto? Producto { get; set; } = null!;
		[EpplusIgnore]
		public DateTime? FchCierre { get; set; } = DateTime.MinValue;
        [EpplusIgnore]
		public DateTime? FchCierre_Vencimiento { get; set; } = DateTime.MinValue;
        [EpplusIgnore]
		public Grupo? Grupo { get; set; } = null!;
        [EpplusIgnore]
        public string? Estado { get; set; } = null!;
        [EpplusIgnore]
        public OrdenEmbarque? ordenEmbarque { get; set; } = null!;
       

        [DisplayName("Fecha de Cierre")]
        public string FchCie { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:d}", FchCierre); } }
        [DisplayName("Fecha de vencimiento")]
        public string FchCie_Ven { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:d}", FchCierre); } }
        [DisplayName("Grupo")]
		public string NGrupo { get { return Grupo != null ? Grupo.Den! : string.Empty; } } 
        [DisplayName("Cliente")]
		public string NCliente { get { return cliente != null ? cliente.Den! : string.Empty; } }
        [DisplayName("Destino")]
		public string NDestino { get { return destino != null ? destino.Den! : string.Empty; } }
        [DisplayName("Producto")]
		public string NProducto { get { return Producto != null ? Producto.Den! : string.Empty; } }

        //Estatus del cierre Activa-Cerrada
        [NotMapped, EpplusIgnore]
        public bool? Activa { get; set; } = true;
        //Volumenes
        [EpplusIgnore]
        public int? Volumen { get; set; }
        [DisplayName("Volumen Total"), NotMapped]
        public string Volumenes { get { return string.Format(new System.Globalization.CultureInfo("es-MX"), "{0:N2}", Volumen); } }
        [EpplusIgnore]
        public double? Volumen_Disponible { get; set; }
        [DisplayName("Volumen Disponible"), NotMapped]
        public string VolumenDisponible { get { return string.Format(new System.Globalization.CultureInfo("es-MX"), "{0:N2}", Volumen_Disponible); } }
        //Precio
        [NotMapped]
        public double Precio { get; set; } = 0;
        //Observaciones
        public string? Observaciones { get; set; } = string.Empty;
    }
}

