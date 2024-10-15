using AutoMapper;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileModel : Profile
    {
        protected MapperProfileModel()
        {
            CreateMap<Inventario, InventarioDTO>();
            CreateMap<Inventario, InventarioPostDTO>();
            CreateMap<InventarioPostDTO, Inventario>()
                .ForMember(x => x.FechaRegistro, op => op.Ignore())
                .ForMember(x => x.Activo, op => op.Ignore());
        }
    }
}
