using AutoMapper;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileModel : Profile
    {
        public MapperProfileModel()
        {
            CreateMap<Inventario, InventarioDTO>();
            CreateMap<Inventario, InventarioPostDTO>();
            CreateMap<InventarioPostDTO, Inventario>()
                .ForMember(x => x.FechaRegistro, op => op.Ignore())
                .ForMember(x => x.FechaCierre, op => op.Ignore())
                .ForMember(x => x.Activo, op => op.Ignore());

            CreateMap<Tad, TerminalDTO>();
            CreateMap<Tad, TerminalPostDTO>();
            CreateMap<TerminalPostDTO, Tad>();

            CreateMap<InventarioCierre, InventarioCierreDTO>();
        }
    }
}
