using AutoMapper;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;

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
        }
    }
}
