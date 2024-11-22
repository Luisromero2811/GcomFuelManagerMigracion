using AutoMapper;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileModel : Profile
    {
        public MapperProfileModel()
        {
            CreateMap<Destino, Destino>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore());
            CreateMap<Destino, DestinoDTO>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore())
                .ForMember(x => x.Codsyn, opt => opt.Ignore());
            CreateMap<DestinoPostDTO, Destino>()
                .ForMember(x => x.Id_Tad, opt => opt.Ignore());
            CreateMap<Destino, DestinoPostDTO>();

            CreateMap<Cliente, ClienteDTO>();

            CreateMap<Vendedor, VendedorDTO>();

            CreateMap<Originador, OriginadorDTO>();
        }
    }
}