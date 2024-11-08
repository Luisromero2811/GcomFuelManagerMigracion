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
                .ForMember(x => x.TipoMovimientoId, op => op.Ignore());
            CreateMap<Inventario, InventarioPostDTO>();
            CreateMap<InventarioPostDTO, Inventario>()
                .ForMember(x => x.FechaRegistro, op => op.Ignore())
                .ForMember(x => x.FechaCierre, op => op.Ignore())
                .ForMember(x => x.Activo, op => op.Ignore());

            CreateMap<Tad, TerminalDTO>();
            CreateMap<Tad, TerminalPostDTO>();
            CreateMap<TerminalPostDTO, Tad>();

            CreateMap<InventarioCierre, InventarioCierreDTO>();
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
        }
    }
}
