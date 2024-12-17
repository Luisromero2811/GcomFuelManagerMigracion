using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OfficeOpenXml.Attributes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;

namespace GComFuelManager.Shared.Modelos
{
    public class OrdenEmbarque
    {
        [Key] public int Cod { get; set; }
        public DateTime? FchOrd { get; set; }
        public DateTime? FchPro { get; set; }
        public Int16? Codtad { get; set; } = 1;
        public byte? Codprd { get; set; }

        [DisplayName("Volumen"), EpplusIgnore]
        public double? Vol { get; set; } = 0;
        [DisplayName("Volumen")]
        public string Volumenes { get { return string.Format(new System.Globalization.CultureInfo("en-US"), "{0:N2}", Vol); } }
        public int? Codchf { get; set; }
        public int? Coddes { get; set; }
        public byte? Codest { get; set; }
        public DateTime? Fchpet { get; set; }
        public DateTime? Fchcar { get; set; } = DateTime.Today;
        public int? Codton { get; set; }
        public int? Bin { get; set; }
        public int? Codusu { get; set; }
        public int? Folio { get; set; }
        public double? Pre { get; set; } = 0;
        public int? CodordCom { get; set; }
        public string? Bolguidid { get; set; }
        public bool? Tp { get; set; }
        public int? CompartmentId { get; set; }
        public int? Compartment { get; set; } = 1;
        public int? NumTonel { get; set; }
        [EpplusIgnore] public int? ID_Moneda { get; set; } = 0;
        public double? Equibalencia { get; set; } = 1;
        public string? FolioSyn { get; set; } = string.Empty;
        [EpplusIgnore]
        public int? Bol { get; set; }
        public byte? Estatus { get; set; }
        public int? Id_Autorizador { get; set; }
        public int? Folio_Vale { get; set; }
        public int? Id_Multidestino { get; set; }
        public ModeloVenta? Modelo_Venta_Orden { get; set; } = ModeloVenta.Rack;
        public TipoCompra? ModeloCompra { get; set; } = TipoCompra.Rack;
        public TipoVenta? TipoVenta { get; set; } = Enums.TipoVenta.Rack;

        [NotMapped] public List<HistorialEstados> HistorialEstados { get; set; } = new();
        [NotMapped] public Destino? Destino { get; set; } = null!;
        [NotMapped] public Tad? Tad { get; set; } = null!;
        [NotMapped] public Producto? Producto { get; set; } = null!;
        [NotMapped] public Tonel? Tonel { get; set; } = null!;
        [NotMapped] public Chofer? Chofer { get; set; } = null!;
        [EpplusIgnore, NotMapped] public Moneda? Moneda { get; set; } = null!;
        [NotMapped] public OrdenCompra? OrdenCompra { get; set; } = null!;
        [NotMapped] public Estado? Estado { get; set; } = null!;
        [NotMapped] public Estado? Estatus_Orden { get; set; } = null!;
        [NotMapped] public Cliente? Cliente { get; set; } = null!;
        [NotMapped] public Usuario? Usuario { get; set; } = null!;
        [NotMapped] public Orden? Orden { get; set; } = null!;
        [NotMapped] public Transportista? Transportista { get; set; } = null!;
        [NotMapped] public OrdenCierre? OrdenCierre { get; set; } = null!;
        [NotMapped] public OrdenPedido? OrdenPedido { get; set; } = null!;
        [NotMapped] public Datos_Facturas? Datos_Facturas { get; set; } = null!;
        [NotMapped] public List<Archivo>? Archivos { get; set; } = null!;
        public OrdenEmbarque ShallowCopy()
        {
            return (OrdenEmbarque)this.MemberwiseClone();
        }
        public OrdenEmbarque HardCopy()
        {
            return new()
            {
                Cod = Cod,
                Codchf = Codchf,
                Coddes = Coddes,
                Codest = Codest,
                CodordCom = CodordCom,
                Codprd = Codprd,
                Codtad = Codtad,
                Codton = Codton,
                Codusu = Codusu,
                Fchcar = Fchcar,
                FchOrd = FchOrd,
                Fchpet = DateTime.Now,
                FchPro = FchPro,
                Folio = Folio,
                FolioSyn = FolioSyn,
                Bin = Bin,
                Pre = Pre,
                Bolguidid = Bolguidid,
                Compartment = Compartment,
                CompartmentId = CompartmentId,
                NumTonel = NumTonel,
                ID_Moneda = ID_Moneda,
                Equibalencia = Equibalencia,
                Bol = Bol,
                Vol = Vol,
                Estatus = Estatus
            };
        }

        [NotMapped, EpplusIgnore] public int Ordenes_A_Crear { get; set; } = 1;
        [NotMapped, EpplusIgnore] public double Costo { get; set; } = 0;
        [NotMapped, EpplusIgnore] public double Utilidad { get; set; } = 0;
        [NotMapped, EpplusIgnore] public double? Utilidad_Sobre_Volumen { get; set; } = 0;
        [NotMapped, EpplusIgnore] public bool Mostrar_Detalle_Orden { get; set; } = false;
        [NotMapped, EpplusIgnore] public List<Orden> Ordenes_Synthesis { get; set; } = new();

        public double Obtener_Utilidad_Coste()
        {
            Utilidad = ((Pre ?? 0) - Costo);
            return Utilidad;
        }
        public double? Obtener_Utilidad_Sobre_Volumen()
        {
            var vol = Compartment == 1 && Tonel != null ? double.Parse(Tonel!.Capcom!.ToString() ?? "0")
                                        : Compartment == 2 && Tonel != null ? double.Parse(Tonel!.Capcom2!.ToString() ?? "0")
                                        : Compartment == 3 && Tonel != null ? double.Parse(Tonel!.Capcom3!.ToString() ?? "0")
                                        : Compartment == 4 && Tonel != null ? double.Parse(Tonel!.Capcom4!.ToString() ?? "0")
                                        : Vol ?? 0;
            Utilidad_Sobre_Volumen = vol * Utilidad;
            return Utilidad_Sobre_Volumen;
        }
        public double Obtener_Volumen_De_Orden()
        {
            try
            {
                if (Orden is not null)
                    if (Orden.Vol is not null)
                        return (double)Orden.Vol;

                if (Tonel is not null && Codest != 41)
                {
                    if (Compartment == 1) return double.Parse(Tonel.Capcom.ToString() ?? "0");
                    if (Compartment == 2) return double.Parse(Tonel.Capcom2.ToString() ?? "0");
                    if (Compartment == 3) return double.Parse(Tonel.Capcom3.ToString() ?? "0");
                    if (Compartment == 4) return double.Parse(Tonel.Capcom4.ToString() ?? "0");
                }

                if (Vol is not null)
                    return (double)Vol;

                return 0;
            }
            catch (Exception)
            {
                return 0;
            }
        }
        public string Obtener_Volumen_De_Orden_En_Formato()
        {
            try
            {
                return Obtener_Volumen_De_Orden().ToString("N", CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                return "0";
            }
        }
        public Gestión_EstadosDTO Obtener_Orden_Gestion_Estado()
        {
            Gestión_EstadosDTO gestion_ = new();
            if (Orden?.OrdEmbDet?.Id_Tad != 1)
            {
                gestion_.Referencia = FolioSyn;
                gestion_.FechaPrograma = Fchcar?.ToString("yyyy-MM-dd");

                if (Tad is not null)
                    if (!string.IsNullOrEmpty(Tad.Den))
                        gestion_.Unidad_Negocio = Tad.Den;

                gestion_.EstatusOrden = Obtener_Estado_De_Orden;

                gestion_.FechaCarga = Obtener_Fecha_De_Carga_De_Orden.ToString("yyyy-MM-dd HH:mm:ss");
                gestion_.Bol = Orden?.BatchId;
                gestion_.MdVenta = Modelo_Venta_Orden.ToString();
                //gestion_.DeliveryRack = Destino?.Cliente?.Tipven ?? string.Empty;
                gestion_.Cliente = Obtener_Cliente_De_Orden;
                gestion_.Destino = Obtener_Destino_De_Orden;
                gestion_.Producto = Obtener_Producto_De_Orden;
                if (Tonel is not null)
                    gestion_.VolNat = Compartment == 1 ? Convert.ToDouble(Tonel.Capcom) :
                            Compartment == 2 ? Convert.ToDouble(Tonel.Capcom2) :
                            Compartment == 3 ? Convert.ToDouble(Tonel.Capcom3) :
                            Compartment == 4 ? Convert.ToDouble(Tonel.Capcom4) : Vol;
                gestion_.VolCar = Orden?.Vol;
                gestion_.Transportista = Tonel?.Transportista?.Den;
                gestion_.Unidad = Obtener_Tonel_De_Orden;
                gestion_.Operador = Chofer?.FullName;
                gestion_.Numero_Factura = Datos_Facturas?.Numero_Orden;
                gestion_.Factura_MGC = Datos_Facturas?.Factura_MGC;
                gestion_.Factura_MexicoS = Datos_Facturas?.Factura_MexicoS;
                gestion_.Factura_DCL = Datos_Facturas?.Factura_DCL;
                gestion_.Factura_Energas = Datos_Facturas?.Factura_Energas;
                if (HistorialEstados is not null)
                {
                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "1. Asignado") is not null)
                        gestion_.Asignado = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "1. Asignado").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "2. Cargado") is not null)
                        gestion_.Cargado = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "2. Cargado").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "3. En tránsito a destino") is not null)
                        gestion_.Transito_Destino = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "3. En tránsito a destino").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "4. Fuera de destino") is not null)
                        gestion_.Fuera_Destino = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "4. Fuera de destino").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "5. Dentro de destino") is not null)
                        gestion_.Dentro_Destino = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "5. Dentro de destino").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "6. En proceso de descarga") is not null)
                        gestion_.Proceso_Descarga = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "6. En proceso de descarga").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "7. Descargado") is not null)
                        gestion_.Descargado = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "7. Descargado").Fecha_Actualizacion.ToString();

                    if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "8. Orden Cancelada") is not null)
                        gestion_.Orden_Cancelada = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "8. Orden Cancelada").Fecha_Actualizacion.ToString();

                    //if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "9_Descargado") is not null)
                    //    gestion_.Descargado = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "9_Descargado").Fecha_Actualizacion.ToString();

                    //if (HistorialEstados.FirstOrDefault(x => x.Estado != null && x.Estado.den == "10_Orden cancelada") is not null)
                    //    gestion_.Orden_Cancelada = HistorialEstados.First(x => x.Estado != null && x.Estado.den == "10_Orden cancelada").Fecha_Actualizacion.ToString();

                }
            }
            return gestion_;
        }
        public EtaNTDTO Obtener_OrdenesETA()
        {
            EtaNTDTO gestion_ = new();
            if (Orden?.OrdEmbDet?.Id_Tad != 1)
            {
                gestion_.Referencia = FolioSyn;
                gestion_.FechaPrograma = Fchcar?.ToString("yyyy-MM-dd");

                if (Tad is not null)
                    if (!string.IsNullOrEmpty(Tad.Den))
                        gestion_.Unidad_Negocio = Tad.Den;

                gestion_.EstatusOrden = Obtener_Estado_De_Orden;

                gestion_.FechaCarga = Obtener_Fecha_De_Carga_De_Orden.ToString("yyyy-MM-dd HH:mm:ss");
                gestion_.Bol = Orden?.BatchId;
                //gestion_.MdVenta = Modelo_Venta_Orden;
                gestion_.Cliente = Obtener_Cliente_De_Orden;
                gestion_.Destino = Obtener_Destino_De_Orden;
                gestion_.Producto = Obtener_Producto_De_Orden;
                //if (Tonel is not null)
                //    gestion_.VolNat = Compartment == 1 ? Convert.ToDouble(Tonel.Capcom) :
                //            Compartment == 2 ? Convert.ToDouble(Tonel.Capcom2) :
                //            Compartment == 3 ? Convert.ToDouble(Tonel.Capcom3) :
                //            Compartment == 4 ? Convert.ToDouble(Tonel.Capcom4) : Vol;
                gestion_.VolCar = Orden?.Vol;
                gestion_.Transportista = Tonel?.Transportista?.Den;
                gestion_.Unidad = Obtener_Tonel_De_Orden;
                gestion_.Operador = Chofer?.FullName;
                gestion_.Sellos = Orden?.SealNumber;
                gestion_.Pedimentos = Orden?.Pedimento;
                gestion_.NOrden = Orden?.NOrden;
                gestion_.Factura = Orden?.Factura;
                gestion_.Importe = Orden?.Importe;
                gestion_.Fecha_llegada = Orden?.OrdEmbDet?.Fchlleest?.ToString("dd-MM-yyyy hh:mm:ss");
                gestion_.Precio = Pre;
            }


            return gestion_;
        }

        public EtaDTO Obtener_OrdenesETATuxpan()
        {
            EtaDTO gestion_ = new();
            gestion_.Referencia = FolioSyn;
            gestion_.FechaPrograma = Fchcar?.ToString("yyyy-MM-dd");

            if (Tad is not null)
                if (!string.IsNullOrEmpty(Tad.Den))
                    gestion_.Unidad_Negocio = Tad.Den;

            gestion_.EstatusOrden = Obtener_Estado_De_Orden;

            gestion_.FechaCarga = Obtener_Fecha_De_Carga_De_Orden.ToString("yyyy-MM-dd HH:mm:ss");
            gestion_.Bol = Orden?.BatchId;
            gestion_.MdVenta = Modelo_Venta_Orden.ToString();
            gestion_.Cliente = Obtener_Cliente_De_Orden;
            gestion_.Destino = Obtener_Destino_De_Orden;
            gestion_.Producto = Obtener_Producto_De_Orden;
            if (Tonel is not null)
                gestion_.VolNat = Compartment == 1 ? Convert.ToDouble(Tonel.Capcom) :
                        Compartment == 2 ? Convert.ToDouble(Tonel.Capcom2) :
                        Compartment == 3 ? Convert.ToDouble(Tonel.Capcom3) :
                        Compartment == 4 ? Convert.ToDouble(Tonel.Capcom4) : Vol;
            gestion_.VolCar = Orden?.Vol;
            gestion_.Transportista = Tonel?.Transportista?.Den;
            gestion_.Unidad = Obtener_Tonel_De_Orden;
            gestion_.Operador = Chofer?.FullName;
            gestion_.FechaDoc = Orden?.OrdEmbDet?.FchDoc.ToString();
            gestion_.Eta = Orden?.OrdEmbDet?.FchDoc!.Value.Subtract(Orden.OrdEmbDet.Fchlleest.Value!).ToString("hh\\:mm");
            gestion_.FechaEst = Orden?.OrdEmbDet?.Fchlleest.ToString();
            return gestion_;
        }


        public string Obtener_Cliente_De_Orden
        {
            get
            {
                if (Orden is not null)
                {
                    if (Orden.Redireccionamiento is not null)
                        if (Orden.Redireccionamiento.Cliente is not null)
                            if (!string.IsNullOrEmpty(Orden.Redireccionamiento.Cliente.Den))
                                return Orden.Redireccionamiento.Cliente.Den;

                    if (Orden.Destino is not null)
                        if (Orden.Destino.Cliente is not null)
                            if (!string.IsNullOrEmpty(Orden.Destino.Cliente.Den))
                                return Orden.Destino.Cliente.Den;
                }

                if (OrdenCierre is not null)
                    if (OrdenCierre.Cliente is not null)
                        if (!string.IsNullOrEmpty(OrdenCierre.Cliente.Den))
                            return OrdenCierre.Cliente.Den;

                if (Destino is not null)
                {
                    if (!string.IsNullOrEmpty(Destino.Den))
                    {
                        Cliente? cliente = Destino.Cliente;
                        return cliente?.Den;
                    }
                }

                return "Sin cliente asignado";
            }
        }
        public string Obtener_Destino_De_Orden
        {
            get
            {
                if (Orden is not null)
                {
                    if (Orden.Redireccionamiento is not null)
                        if (Orden.Redireccionamiento.Destino is not null)
                            if (!string.IsNullOrEmpty(Orden.Redireccionamiento.Destino.Den))
                                return Orden.Redireccionamiento.Destino.Den;

                    if (Orden.Destino is not null)
                        if (!string.IsNullOrEmpty(Orden.Destino.Den))
                            return Orden.Destino.Den;
                }

                if (Destino is not null)
                    if (!string.IsNullOrEmpty(Destino.Den))
                        return Destino.Den;

                return "Sin destino asignado";
            }
        }
        public string Obtener_Producto_De_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.Producto is not null)
                        if (!string.IsNullOrEmpty(Orden.Producto.Den))
                            return Orden.Producto.Den;

                if (Producto is not null)
                    if (!string.IsNullOrEmpty(Producto.Den))
                        return Producto.Den;

                return "Sin producto asignado";
            }
        }
        public string Obtener_Estado_De_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.Estado is not null)
                        if (!string.IsNullOrEmpty(Orden.Estado.den))
                            return Orden.Estado.den;

                if (Estado is not null)
                    if (!string.IsNullOrEmpty(Estado.den))
                        return Estado.den;

                return "Sin estado asignado";
            }
        }
        public DateTime Obtener_Fecha_De_Carga_De_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.Fchcar is not null)
                        return (DateTime)Orden.Fchcar;

                //if (Fchcar is not null)
                //    return (DateTime)Fchcar;

                return DateTime.MinValue;
            }
        }
        public string Obtener_Tonel_De_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.Tonel is not null)
                        if (!string.IsNullOrEmpty(Orden.Tonel.Veh))
                            return Orden.Tonel.Veh;

                if (Tonel is not null)
                    if (!string.IsNullOrEmpty(Tonel.Veh))
                        return Tonel.Veh;

                return "Si unidad asignada";
            }
        }
        public string Obtener_Modelo_Venta_Orden
        {
            get
            {
                if (Orden is not null)
                    if (Orden.Destino is not null)
                        if (Orden.Destino.Cliente is not null)
                            if (!string.IsNullOrEmpty(Orden.Destino.Cliente.Tipven))
                                return Orden.Destino.Cliente.Tipven;

                if (Destino is not null)
                    if (Destino.Cliente is not null)
                        if (!string.IsNullOrEmpty(Destino.Cliente.Tipven))
                            return Destino.Cliente.Tipven;

                return string.Empty;
            }
        }
    }

    public class OrdenEmbarque_Excel
    {
        [DisplayName("Fecha de carga")]
        public DateTime Fecha_Programa { get; set; } = DateTime.MinValue;
        [DisplayName("Fecha de llegada estimada")]
        public DateTime Fecha_Estimada { get; set; } = DateTime.MinValue;
        public string Producto { get; set; } = string.Empty;
        public double Volumen { get; set; } = 0;
        public string Cliente { get; set; } = string.Empty;
        public string Destino { get; set; } = string.Empty;
        //public string Tonel { get; set; } = string.Empty;
        //public int Compartimento { get; set; } = 0;
        //public string Operador { get; set; } = string.Empty;
        public string Turno { get; set; } = string.Empty;

    }
}
