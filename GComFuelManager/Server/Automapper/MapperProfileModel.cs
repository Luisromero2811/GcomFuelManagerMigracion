using AutoMapper;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileModel : Profile
    {
        public MapperProfileModel()
        {
            CreateMap<Inventario, Inventario>()
                .ForMember(x => x.Activo, op => op.Ignore())
                .ForMember(x => x.FechaRegistro, op => op.Ignore());
            CreateMap<Inventario, InventarioDTO>()
                .ForMember(x => x.ProductoId, op => op.Ignore())
                .ForMember(x => x.SitioId, op => op.Ignore())
                .ForMember(x => x.AlmacenId, op => op.Ignore())
                .ForMember(x => x.LocalidadId, op => op.Ignore())
                .ForMember(x => x.UnidadMedidaId, op => op.Ignore())
                .ForMember(x => x.TipoMovimientoId, op => op.Ignore())
                .AfterMap((x, y) =>
                {
                    if (int.TryParse(x.TipoMovimiento.Abreviacion, out int tm))
                        if (tm >= 20)
                            y.Cantidad *= -1;
                });
            CreateMap<Inventario, InventarioPostDTO>();
            CreateMap<InventarioPostDTO, Inventario>()
                .ForMember(x => x.FechaRegistro, op => op.Ignore())
                .ForMember(x => x.FechaCierre, op => op.Ignore())
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<Destino, Destino>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore());
            CreateMap<Destino, DestinoDTO>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore())
                .ForMember(x => x.Codsyn, opt => opt.Ignore());
            CreateMap<DestinoPostDTO, Destino>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore());
            CreateMap<Destino, DestinoPostDTO>();

            CreateMap<Cliente, ClienteDTO>();
            CreateMap<Tad, TerminalDTO>();
            CreateMap<Tad, TerminalPostDTO>();
            CreateMap<TerminalPostDTO, Tad>();

            CreateMap<Vendedor, VendedorDTO>();
            CreateMap<InventarioCierre, InventarioCierre>(); 
            CreateMap<InventarioCierre, InventarioCierreDTO>()
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
                });
            CreateMap<InventarioCierreDTO, InventarioCierre>()
                .ForMember(x => x.Producto, op => op.Ignore())
                .ForMember(x => x.Sitio, op => op.Ignore())
                .ForMember(x => x.Almacen, op => op.Ignore())
                .ForMember(x => x.Localidad, op => op.Ignore());

            CreateMap<Catalogo_Fijo, CatalogoValorDTO>()
                .ForMember(x => x.CatalogoId, op => op.MapFrom(x => x.Catalogo));

            CreateMap<Catalogo, CatalogoDTO>();
            CreateMap<CatalogoValor, CatalogoValorDTO>();
            CreateMap<CatalogoValor, CatalogoValor>()
                .ForMember(x => x.Code, op => op.Ignore())
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CatalogoValor, CatalogoValorPostDTO>();
            CreateMap<CatalogoValorPostDTO, CatalogoValor>();

            CreateMap<Originador, OriginadorDTO>();
            CreateMap<Usuario, UsuarioSistemaDTO>();
        }
    }
}
