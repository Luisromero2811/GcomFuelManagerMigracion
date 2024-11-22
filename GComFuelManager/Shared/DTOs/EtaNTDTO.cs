using System;
using GComFuelManager.Shared.Modelos;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using GComFuelManager.Shared.Filtro;
using GComFuelManager.Shared.Enums;
using System.Globalization;

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
        public int? Bol { get; set; }
        public double? Precio { get; set; }
        [DisplayName("Modelo de Venta")]
        public TipoVentaFiltro? MdVenta { get; set; } = TipoVentaFiltro.Todos;
        [DisplayName("Modelo de Compra")]
        public TipoCompraFiltro? ModeloCompra { get; set; } = TipoCompraFiltro.Ambos;
        [DisplayName("Tipo de Venta")]
        public string DeliveryRack { get; set; } = string.Empty;
        public string? Cliente { get; set; } = string.Empty;
        public string? Destino { get; set; } = string.Empty;
        public string? Producto { get; set; } = string.Empty;

        [DisplayName("Volumen Natural"), EpplusIgnore]
        public double? VolNat { get; set; } = 0;
        [DisplayName("Volumen Natural")]
        public string Volms => string.Format(CultureInfo.InvariantCulture, "{0:N2}", VolNat);

        [DisplayName("Volumen Cargado"), EpplusIgnore]
        public double? VolCar { get; set; } = 0;
        [DisplayName("Volumen Cargado")]
        public string Vols { get { return string.Format(CultureInfo.InvariantCulture, "{0:N2}", VolCar); } }

        public string? Transportista { get; set; } = string.Empty;
        public string? Unidad { get; set; } = string.Empty;
        public string? Operador { get; set; } = string.Empty;
        public string? ETA
        {
            get
            {
                if (DateTime.TryParse(FechaCarga, out DateTime fecha_carga) && DateTime.TryParse(Fecha_llegada, out DateTime fecha_llegada))
                    return fecha_carga.Subtract(fecha_llegada).ToString("hh\\:mm");

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
        [DisplayName("Importe Total")]
        public string? Importe { get; set; } = string.Empty;
        [EpplusIgnore]
        public Orden? orden { get; set; }
    }
}

