using System;
using GComFuelManager.Shared.Modelos;
using System.ComponentModel;
using OfficeOpenXml.Attributes;

namespace GComFuelManager.Shared.DTOs
{
	public class Gestión_EstadosDTO
	{
        //Propiedades para mandar el rango de fechas como filtro
        [EpplusIgnore] public DateTime DateInicio { get; set; } = DateTime.Today.Date;
        [EpplusIgnore] public DateTime DateFin { get; set; } = DateTime.Now;
        [EpplusIgnore] public int Estado { get; set; } = 1;
        //Propiedades a utilizar en la consulta general y del excel
        public string? Referencia { get; set; } = string.Empty;
        [DisplayName("Fecha de Programa")]
        public string? FechaPrograma { get; set; } = string.Empty;
        [DisplayName("Estatus de Orden")]
        public string? EstatusOrden { get; set; } = string.Empty;
        [DisplayName("Fecha de Carga")]
        public string? FechaCarga { get; set; } = string.Empty;
        public int? Bol { get; set; } = 0;

        [DisplayName("Tipo de Venta")]
        public string DeliveryRack { get; set; } = string.Empty;

        public string? Cliente { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;

        [DisplayName("Volumen Natural"), EpplusIgnore]
        public double? VolNat { get; set; } = 0;
        [DisplayName("Volumen Natural")]
        public string Volms { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", VolNat); } }

        [DisplayName("Volumen Cargado"), EpplusIgnore]
        public double? VolCar { get; set; } = 0;
        [DisplayName("Volumen Cargado")]
        public string Vols { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", VolCar); } }

        public string? Transportista { get; set; } = string.Empty;
        public string? Unidad { get; set; } = string.Empty;
        public string? Operador { get; set; } = string.Empty;

        [DisplayName("Unidad de Negocio")]
        public string? Unidad_Negocio { get; set; } = string.Empty;
        [DisplayName("Numero de orden")]
        public string? Numero_Factura { get; set; } = string.Empty;
        [DisplayName("Factura MGC")]
        public string? Factura_MGC { get; set; } = string.Empty;
        [DisplayName("Factura Mexico S")]
        public string? Factura_MexicoS { get; set; } = string.Empty;
        [DisplayName("Factura DCL")]
        public string? Factura_DCL { get; set; } = string.Empty;
        [DisplayName("Factura Energas")]
        public string? Factura_Energas { get; set; } = string.Empty;
        [DisplayName("Asignado")]
        public string? Asignado { get; set; } = string.Empty;

        [DisplayName("Cargado")]
        public string? Cargado { get; set; } = string.Empty;

        [DisplayName("En tránsito a destino")]
        public string? Transito_Destino { get; set; } = string.Empty;

        [DisplayName("Fuera de destino")]
        public string? Fuera_Destino { get; set; } = string.Empty;

        [DisplayName("Dentro de destino")]
        public string? Dentro_Destino { get; set; } = string.Empty;

        [DisplayName("En proceso de descarga")]
        public string? Proceso_Descarga { get; set; } = string.Empty;

        [DisplayName("Descargado")]
        public string? Descargado { get; set; } = string.Empty;

        [DisplayName("Orden Cancelada")]
        public string? Orden_Cancelada { get; set; } = string.Empty;

        //[DisplayName("Descargado")]
        //public string? Descargado { get; set; } = string.Empty;

        //[DisplayName("Orden Cancelada")]
        //public string? Orden_Cancelada { get; set; } = string.Empty;

        //Propiedades de navegación externas
        [EpplusIgnore]
        public Orden? orden { get; set; }
        [EpplusIgnore]
        public OrdenEmbarque? ordenEmbarque { get; set; }
        [EpplusIgnore]
        public HistorialEstados? historialEstados { get; set; }
        [EpplusIgnore]
        public Tad? tad { get; set; }
    }
}

