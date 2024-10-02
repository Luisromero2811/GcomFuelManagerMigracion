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
            CreateMap<CRMOportunidad, CRMOportunidadPostDTO>()
                .ForMember(x => x.DocumentoId, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Id : 0))
                .ForMember(x => x.NombreDocumento, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().NombreDocumento : string.Empty))
                .ForMember(x => x.Version, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Version : string.Empty))
                .ForMember(x => x.FechaCaducidad, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().FechaCaducidad : DateTime.MinValue))
                .ForMember(x => x.Descripcion, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Descripcion : string.Empty));

            CreateMap<CRMOportunidad, CRMOportunidadDetalleDTO>()
                .ForMember(x => x.Documento, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First() : new()));
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
                .ForMember(x => x.Cuenta_Rel, y => y.MapFrom(c => c.contacto.Cliente.Nombre ?? "No Asignado"))
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
            CreateMap<CRMCliente, CRMClienteDetalleDTO>();

            CreateMap<CRMDocumento, CRMDocumento>()
                .ForMember(x => x.Activo, y => y.Ignore())
                .ForMember(x => x.NombreArchivo, y => y.Ignore())
                .ForMember(x => x.FechaCreacion, y => y.Ignore())
                .ForMember(x => x.VersionCreadaPor, y => y.Ignore())
                .ForMember(x => x.TipoDocumento, y => y.Ignore())
                .ForMember(x => x.Url, y => y.Ignore())
                .ForMember(x => x.Directorio, y => y.Ignore());
            CreateMap<CRMOportunidadPostDTO, CRMDocumento>()
                .ForMember(x => x.Id, y => y.MapFrom(doc => doc.DocumentoId));
            CreateMap<CRMDocumento, CRMDocumentoDTO>();
            CreateMap<CRMDocumento, CRMDocumentoDetalleDTO>();

            CreateMap<CRMActividadPostDTO, CRMDocumento>()
                .ForMember(x => x.Id, y => y.MapFrom(doc => doc.DocumentoId));
            CreateMap<CRMDocumento, CRMDocumentoDetalleDTO>()
                .ForMember(x => x.VersionCreadaPor, y => y.Ignore());

            //Actividades
            CreateMap<CRMActividades, CRMActividadPostDTO>()
                .ForMember(x => x.DocumentoId, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Id : 0))
                .ForMember(x => x.NombreDocumento, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().NombreDocumento : string.Empty))
                .ForMember(x => x.Version, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Version : string.Empty))
                .ForMember(x => x.FechaCaducidad, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().FechaCaducidad : DateTime.MinValue))
                .ForMember(x => x.Descripcion, y => y.MapFrom(c => c.Documentos.Count > 0 ? c.Documentos.First().Descripcion : string.Empty));


            CreateMap<CRMCatalogo, CRMCatalogoDTO>();
            CreateMap<CRMCatalogoValor, CRMCatalogoValorDTO>();
            CreateMap<CRMCatalogoValor, CRMCatalogoValorPostDTO>();
            CreateMap<CRMCatalogoValorPostDTO, CRMCatalogoValor>();

        }
    }
}
