using AutoMapper;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;
using System.Globalization;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileReporte : Profile
    {
        public MapperProfileReporte()
        {
            CreateMap<Inventario, InventarioExcelDTO>()
                .ForMember(x => x.TipoInventario, opt => opt.MapFrom(y => y.TipoInventario.Description()))
                .AfterMap((x, y) =>
                {
                    if (int.TryParse(x.TipoMovimiento.Abreviacion, out int tm))
                        if (tm >= 20)
                            y.Cantidad *= -1;
                });
            CreateMap<InventarioCierre, InventarioCierreExcelDTO>()
                .ForMember(x => x.OrdenReservado, op =>
                {
                    op.PreCondition(x => x.OrdenReservado > 0);
                    op.MapFrom(x => x.OrdenReservado * -1);
                })
                .ForMember(x => x.EnOrden, op =>
                {
                    op.PreCondition(x => x.EnOrden > 0);
                    op.MapFrom(x => x.EnOrden * -1);
                })
                .ForMember(x => x.Cargado, op =>
                {
                    op.PreCondition(x => x.Cargado > 0);
                    op.MapFrom(x => x.Cargado * -1);
                })
                //.ForMember(x => x.ReservadoDisponible, op =>
                //{
                //    op.PreCondition(x => x.ReservadoDisponible > 0);
                //    op.MapFrom(x => x.ReservadoDisponible * -1);
                //})
                .ForMember(x => x.Reservado, op =>
                {
                    op.PreCondition(x => x.Reservado > 0);
                    op.MapFrom(x => x.Reservado * -1);
                }); ;
            CreateMap<InventarioCierreDTO, InventarioActualExcelDTO>();

            CreateMap<OrdenEmbarque, EtaNTDTO>()
                .ForMember(x => x.Referencia, opt => opt.MapFrom(x => x.FolioSyn))
                .ForMember(x => x.FechaPrograma, opt => opt.MapFrom(x => $"{x.Fchcar:yyyy-MM-dd}"))
                .ForMember(x => x.Unidad_Negocio, opt =>
                {
                    opt.PreCondition(x => x.Tad != null);
                    opt.MapFrom(x => x.Tad!.Den);
                })
                .ForMember(x => x.EstatusOrden, opt =>
                {
                    opt.PreCondition(x => x.Estado != null);
                    opt.MapFrom(x => x.Estado!.den);
                    opt.NullSubstitute(string.Empty); ;
                })
                .AfterMap((x, y) =>
                {
                    if (x.Orden?.Estado is not null)
                        y.EstatusOrden = x.Orden.Estado.den;
                })
                .ForMember(x => x.FechaCarga, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => $"{x.Orden!.Fchcar:yyyy-MM-dd hh:mm:ss tt}");
                    opt.NullSubstitute(string.Empty);
                })
                .ForMember(x => x.Bol, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.BatchId);
                })
                .ForMember(x => x.MdVenta, opt =>
                {
                    opt.PreCondition(x => x.Destino != null);
                    opt.MapFrom(x => x.Destino!.ModeloVenta);
                })
                .ForMember(x => x.ModeloCompra, opt => opt.MapFrom(x => x.ModeloCompra))
                .ForMember(x => x.Cliente, opt =>
                {
                    opt.PreCondition(x => x.Destino?.Cliente != null);
                    opt.MapFrom(x => x.Destino!.Cliente!.Den);
                    opt.NullSubstitute("Sin cliente asignado");
                })
                .AfterMap((x, y) =>
                {
                    if (x.Orden is not null)
                    {
                        if (x.Orden.Redireccionamiento?.Cliente is not null)
                            y.Cliente = x.Orden.Redireccionamiento.Cliente.Den;

                        if (x.Orden.Destino?.Cliente is not null)
                            y.Cliente = x.Orden.Destino.Cliente.Den;
                    }

                    if (x.OrdenCierre?.Cliente is not null)
                        y.Cliente = x.OrdenCierre.Cliente.Den;
                })
                .ForMember(x => x.Destino, opt =>
                {
                    opt.PreCondition(x => x.Destino != null);
                    opt.MapFrom(x => x.Destino!.Den);
                    opt.NullSubstitute("Sin destino asignado");
                })
                .AfterMap((x, y) =>
                {
                    if (x.Orden is not null)
                    {
                        if (x.Orden.Redireccionamiento?.Destino is not null)
                            y.Destino = x.Orden.Redireccionamiento.Destino.Den;

                        if (x.Orden.Destino is not null)
                            y.Destino = x.Orden.Destino.Den;
                    }
                })
                .ForMember(x => x.Producto, opt =>
                {
                    opt.PreCondition(x => x.Producto != null);
                    opt.MapFrom(x => x.Producto!.Den);
                    opt.NullSubstitute("Sin producto asignado");
                })
                .AfterMap((x, y) =>
                {
                    if (x.Orden?.Producto is not null)
                        y.Producto = x.Orden.Producto.Den;
                })
                .ForMember(x => x.Volms, opt => opt.MapFrom(x => x.Vol))
                .AfterMap((x, y) =>
                {
                    if (x.Tonel is not null)
                    {
                        if (x.Compartment == 1)
                            y.VolNat = double.Parse(x.Tonel.Capcom.ToString() ?? string.Empty);
                        if (x.Compartment == 2)
                            y.VolNat = double.Parse(x.Tonel.Capcom2.ToString() ?? string.Empty);
                        if (x.Compartment == 3)
                            y.VolNat = double.Parse(x.Tonel.Capcom3.ToString() ?? string.Empty);
                        if (x.Compartment == 4)
                            y.VolNat = double.Parse(x.Tonel.Capcom4.ToString() ?? string.Empty);
                    }
                })
                .ForMember(x => x.VolCar, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.Vol);
                })
                .ForMember(x => x.Transportista, opt =>
                {
                    opt.PreCondition(x => x.Tonel?.Transportista != null);
                    opt.MapFrom(x => x.Tonel!.Transportista!.Den);
                })
                .ForMember(x => x.Unidad, opt =>
                {
                    opt.PreCondition(x => x.Tonel != null);
                    opt.MapFrom(x => x.Tonel!.Veh);
                })
                .AfterMap((x, y) =>
                {
                    if (x.Orden?.Tonel != null)
                        y.Unidad = x.Orden.Tonel.Veh;
                })
                .ForMember(x => x.Operador, opt => opt.MapFrom(x => x.Chofer))
                .ForMember(x => x.Sellos, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.SealNumber);
                })
                .ForMember(x => x.Pedimentos, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.Pedimento);
                })
                .ForMember(x => x.NOrden, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.NOrden);
                })
                .ForMember(x => x.Factura, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.Factura);
                })
                .ForMember(x => x.Importe, opt =>
                {
                    opt.PreCondition(x => x.Orden != null);
                    opt.MapFrom(x => x.Orden!.Importe);
                })
                .ForMember(x => x.Fecha_llegada, opt =>
                {
                    opt.PreCondition(x => x.Orden?.OrdEmbDet != null);
                    opt.MapFrom(x => $"{x.Orden!.OrdEmbDet!.Fchlleest:dd-MM-yyyy hh:mm:ss tt}");
                    opt.NullSubstitute(string.Empty);
                })
                .ForMember(x => x.Precio, opt => opt.MapFrom(x => x.Pre));

            CreateMap<Cliente, CatalogoClienteDTO>();
            CreateMap<Destino, CatalogoDestinoDTO>();
            CreateMap<GrupoTransportista, CatalogoGrupoTransportistaDTO>();
            CreateMap<Transportista, CatalogoTransportistaDTO>();
            CreateMap<Chofer, CatalogoChoferDTO>();
            CreateMap<Tonel, CatalogoVehiculoDTO>();
            CreateMap<InventarioCierreDTO, InventarioConsolidacionDTO>();
        }
    }
}
