using AutoMapper;
using GComFuelManager.Shared.DTOs.Especiales;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Mappers;
public class MapperProfileEspecial : Profile
{
    public MapperProfileEspecial()
    {
        CreateMap<CRMDocumento, CRMDocumentoClienteDTO>()
            .ForMember(x => x.ClienteActividad, opt =>
            {
                opt.PreCondition(y => y.Actividades.Count > 0);
                opt.MapFrom(y => y.Actividades.First().Contacto.Cliente.Nombre);
            })
            .ForMember(x => x.ClienteOportunidad, opt =>
            {
                opt.PreCondition(y => y.Oportunidades.Count > 0);
                opt.MapFrom(y => y.Oportunidades.First().CRMCliente.Nombre);
            })
            .ForMember(x => x.ActividadId, opt =>
            {
                opt.PreCondition(y => y.Actividades.Count > 0);
                opt.MapFrom(y => y.Actividades.First().Id);
            })
            .ForMember(x => x.OportunidadId, opt =>
            {
                opt.PreCondition(y => y.Oportunidades.Count > 0);
                opt.MapFrom(y => y.Oportunidades.First().Id);
            })
            .ForMember(x => x.Oportunidad, opt =>
            {
                opt.PreCondition(y => y.Oportunidades.Count > 0);
                opt.MapFrom(y => y.Oportunidades.First().Nombre_Opor);
            })
            .ForMember(x => x.Actividad, opt =>
            {
                opt.PreCondition(y => y.Actividades.Count > 0);
                opt.MapFrom(y => y.Actividades.First().Asuntos.Valor);
            });
    }
}
