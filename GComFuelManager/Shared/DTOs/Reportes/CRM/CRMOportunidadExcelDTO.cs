using System.ComponentModel;

namespace GComFuelManager.Shared.DTOs.Reportes.CRM
{
    public class CRMOportunidadExcelDTO
    {
        public string Oportunidad { get; set; } = string.Empty;
        [DisplayName("Origen de producto")]
        public string OrigenProducto { get; set; } = string.Empty;
        [DisplayName("Tipo de producto")]
        public string TipoProducto { get; set; } = string.Empty;
        [DisplayName("Valor de la oportunidad")]
        public double ValorOportunidad { get; set; } 
        public string Vendedor { get; set; } = string.Empty;
        public string Division { get; set; } = string.Empty;
        public string Equipo { get; set; } = string.Empty;
        public string Cuenta { get; set; } = string.Empty;
        public string Contacto { get; set; } = string.Empty;
        [DisplayName("Telefono movil")]
        public string Tel_Movil { get; set; } = string.Empty;
        [DisplayName("Telefono de oficina")]
        public string Tel_Oficina { get; set; } = string.Empty;
        [DisplayName("Correo electronico")]
        public string Correo { get; set; } = string.Empty;
        [DisplayName("Unidad de medida")]
        public string UnidadMedida { get; set; } = string.Empty;
        [DisplayName("Cantidad de la oportunidad")]
        public double CantidadOportuniad { get; set; }
        [DisplayName("Precio del producto Mxn/Lts")]
        public double Precio { get; set; }
        [DisplayName("Total de litros")]
        public double TotalLts { get; set; }
        public string Periodo { get; set; } = string.Empty;
        [DisplayName("Relacion comercial")]
        public string RelacionComercial { get; set; } = string.Empty;
        [DisplayName("Modelo de venta")]
        public string ModeloVenta { get; set; } = string.Empty;
        [DisplayName("Volumen de retiro")]
        public string Volumen { get; set; } = string.Empty;
        [DisplayName("Forma de pago")]
        public string FormaPago { get; set; } = string.Empty;
        [DisplayName("Dias de credito")]
        public string DiasPago { get; set; } = string.Empty;
        [DisplayName("Cantidad de estaciones")]
        public int CantidadEstaciones { get; set; }
        [DisplayName("Etapa de venta")]
        public string EtapaVenta { get; set; } = string.Empty;
        [DisplayName("Proximo paso")]
        public string ProximoPaso { get; set; } = string.Empty;
    }
}
