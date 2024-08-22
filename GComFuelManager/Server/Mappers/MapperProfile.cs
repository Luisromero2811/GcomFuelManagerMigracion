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
            CreateMap<CRMContacto, CRMContactoPostDTO>();
            CreateMap<CRMContacto, CRMContactoDTO>()
                .ForMember(x => x.Estatus, y => y.MapFrom(c => c.Estatus.Valor))
                .ForMember(x => x.Cuenta, y => y.MapFrom(c => c.Cliente.Nombre))
                .ForMember(x => x.Vendedor, y => y.MapFrom(c => c.Vendedor.Nombre));
            CreateMap<CRMContacto, CRMContactoDetalleDTO>();

            CreateMap<CRMOportunidadPostDTO, CRMOportunidad>();
            CreateMap<CRMOportunidad, CRMOportunidadPostDTO>();
            CreateMap<CRMOportunidad, CRMOportunidadDTO>()
                .ForMember(x => x.EtapaVenta, y => y.MapFrom(o => o.EtapaVenta.Valor))
                .ForMember(x => x.Vendedor, y => y.MapFrom(o => o.Vendedor.Nombre))
                .ForMember(x => x.Cuenta, y => y.MapFrom(o => o.CRMCliente.Nombre));
        }
    }
}
