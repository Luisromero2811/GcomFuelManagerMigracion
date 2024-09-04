using System;
using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using GComFuelManager.Shared.Filtro;

namespace GComFuelManager.Shared.DTOs
{
    public class EtaNTDTO : Parametros_Busqueda_Gen
    {
        public string? Referencia { get; set; } = string.Empty;
        [DisplayName("Fecha de Programa")]
        public string? FechaPrograma { get; set; } = string.Empty;
        [DisplayName("Estatus de Orden")]
        public string? EstatusOrden { get; set; } = string.Empty;
        [DisplayName("Fecha de Carga")]
        public string? FechaCarga { get; set; } = string.Empty;
        public int? Bol { get; set; } = 0;
        public double? Precio { get; set; } = 0;

        [DisplayName("Modelo de Venta")]
        public string? MdVenta { get; set; } = string.Empty;
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
        public string? ETA
        {
            get
            {
                if(DateTime.TryParse(FechaCarga, out DateTime fecha_carga) && DateTime.TryParse(Fecha_llegada, out DateTime fecha_llegada))
                    return fecha_llegada.Subtract(fecha_carga).ToString("hh\\:mm");

                return string.Empty;
            }
        }

        [DisplayName("Fecha estimada de llegada")]
        public string? Fecha_llegada { get; set; } = string.Empty;
        [DisplayName("Unidad de Negocio")]
        public string? Unidad_Negocio { get; set; } = string.Empty;
        [DisplayName("Sellos")]
        public string? Sellos { get; set; } = string.Empty;
        [DisplayName("Pedimentos")]
        public string? Pedimentos { get; set; } = string.Empty;
        [DisplayName("# de orden")]
        public string? NOrden { get; set; } = string.Empty;
        [DisplayName("Factura Proveedor")]
        public string? Factura { get; set; } = string.Empty;
        public string? Pedimento { get; set; } = string.Empty;
        [DisplayName("Importe Total")]
        public string? Importe { get; set; } = string.Empty;
        // public string? Numero_Orden { get; set; } = string.Empty;
        [EpplusIgnore]
        public Orden? orden { get; set; }
        [EpplusIgnore]
        public OrdenEmbarque? ordenEmbarque { get; set; }
        [EpplusIgnore]
        public string? FechaCargaEmbarque { get { return ordenEmbarque?.Fchcar.ToString(); } }
        [EpplusIgnore]
        public int? Compartimento { get; set; } = null!;
        [EpplusIgnore]
        public decimal? Capcom { get { return ordenEmbarque?.Tonel?.Capcom; } }
        [EpplusIgnore]
        public decimal? Capcom2 { get { return ordenEmbarque?.Tonel?.Capcom2; } }
        [EpplusIgnore]
        public decimal? Capcom3 { get { return ordenEmbarque?.Tonel?.Capcom3; } }
        [EpplusIgnore]
        public decimal? Capcom4 { get { return ordenEmbarque?.Tonel?.Capcom4; } }
        [EpplusIgnore]
        public string? VolumenN { get { return orden?.Volumenes; } }
    }
}

