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
                .ForMember(x => x.Vendedor, y => y.MapFrom(c => c.Vendedor.FullName))
                .ForMember(x => x.Division, y => y.MapFrom(c => c.Division.Nombre));
            CreateMap<CRMContacto, CRMContactoDetalleDTO>();

            CreateMap<CRMOportunidad, CRMOportunidad>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMOportunidadPostDTO, CRMOportunidad>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMOportunidad, CRMOportunidadPostDTO>();
            CreateMap<CRMOportunidad, CRMOportunidadDetalleDTO>();
            CreateMap<CRMOportunidad, CRMOportunidadDTO>()
                .ForMember(x => x.EtapaVenta, y => y.MapFrom(o => o.EtapaVenta.Valor))
                .ForMember(x => x.Vendedor, y => y.MapFrom(o => o.Vendedor.FullName))
                .ForMember(x => x.Cuenta, y => y.MapFrom(o => o.CRMCliente.Nombre))
                .ForMember(x => x.Contacto, y => y.MapFrom(o => $"{o.Contacto.Nombre} {o.Contacto.Apellidos}"))
                .ForMember(x => x.Medida, y => y.MapFrom(o => o.UnidadMedida.Abreviacion))
                .ForMember(x => x.Equipo, y => y.MapFrom(o => o.Equipo.Nombre))
                .ForMember(x => x.Division, y => y.MapFrom(o => o.Equipo.Division.Nombre));

            CreateMap<CRMActividades, CRMActividadPostDTO>();
            CreateMap<CRMActividadDTO, CRMActividades>();
            CreateMap<CRMActividadPostDTO, CRMActividades>();
            CreateMap<CRMActividadDTO, CRMActividades>();
            CreateMap<CRMActividades, CRMActividadDTO>()
                .ForMember(x => x.Estatus, y => y.MapFrom(c => c.Estados.Valor))
                .ForMember(x => x.Asunto, y => y.MapFrom(c => c.asuntos.Valor))
                .ForMember(x => x.Prioridad, y => y.MapFrom(c => c.prioridades.Valor))
                .ForMember(x => x.Contacto_Rel, y => y.MapFrom(c => $"{c.contacto.Nombre} {c.contacto.Apellidos}"))
                .ForMember(x => x.Asignado, y => y.MapFrom(c => c.vendedor.Nombre));

            CreateMap<CRMVendedor, CRMVendedor>()
                .ForMember(x => x.UserId, o => o.Ignore());
            CreateMap<CRMVendedor, CRMVendedorDTO>()
                .ForMember(x => x.NombreDivision, y => y.MapFrom(c => c.Division.Nombre));
            CreateMap<CRMVendedorPostDTO, CRMVendedor>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMVendedor, CRMVendedorPostDTO>();
            CreateMap<CRMVendedor, CRMVendedorDetalleDTO>();
            CreateMap<CRMVendedorDTO, CRMVendedor>();

            CreateMap<CRMOriginador, CRMOriginador>()
                .ForMember(x => x.UserId, o => o.Ignore());
            CreateMap<CRMOriginador, CRMOriginadorDTO>()
                .ForMember(x => x.NombreDivision, y => y.MapFrom(c => c.Division.Nombre));
            CreateMap<CRMOriginadorPostDTO, CRMOriginador>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMOriginador, CRMOriginadorPostDTO>();
            CreateMap<CRMOriginador, CRMOriginadorDetalleDTO>();
            CreateMap<CRMOriginadorDTO, CRMOriginador>();

            CreateMap<CRMRol, CRMRolDTO>();
            CreateMap<CRMRol, CRMRolDetalleDTO>();
            CreateMap<CRMRol, CRMRolPostDTO>();
            CreateMap<CRMRolPostDTO, CRMRol>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMEquipo, CRMEquipo>();
            CreateMap<CRMEquipo, CRMEquipoDTO>()
                .ForMember(x => x.Lider, y => y.MapFrom(e => e.Originador.FullName))
                .ForMember(x => x.TelMovil, y => y.MapFrom(e => e.Originador.Tel_Movil))
                .ForMember(x => x.Correo, y => y.MapFrom(e => e.Originador.Correo))
                .ForMember(x => x.Division, y => y.MapFrom(e => e.Division.Nombre));
            CreateMap<CRMEquipo, CRMEquipoDetalleDTO>();
            CreateMap<CRMEquipo, CRMEquipoPostDTO>();
            CreateMap<CRMEquipoPostDTO, CRMEquipo>();

            CreateMap<CRMCliente, CRMClienteDTO>();
            CreateMap<CRMCliente, CRMClientePostDTO>();
            CreateMap<CRMClientePostDTO, CRMCliente>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMCliente, CRMClienteDetalle>();
        }
    }
}
