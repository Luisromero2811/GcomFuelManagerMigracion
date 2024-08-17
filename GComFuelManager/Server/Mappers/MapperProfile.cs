using AutoMapper;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Mappers
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CRMContactoDTO, CRMContacto>();
            CreateMap<CRMContactoPostDTO, CRMContacto>();
            CreateMap<CRMContacto, CRMContactoDTO>();
        }
    }
}
