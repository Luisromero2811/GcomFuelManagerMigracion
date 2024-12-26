using AutoMapper;
using GComFuelManager.Shared.DTOs.CRM;
using GComFuelManager.Shared.DTOs.Especiales;
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
                .ForMember(x => x.Estatus, y => y.MapFrom(c => c.Estatus != null ? c.Estatus.Valor : string.Empty))
                .ForMember(x => x.Cuenta, y => y.MapFrom(c => c.Cliente != null ? c.Cliente.Nombre : string.Empty))
                .ForMember(x => x.Vendedor, y => y.MapFrom(c => c.Vendedor != null ? c.Vendedor.FullName : string.Empty))
                .ForMember(x => x.Division, y => y.MapFrom(c => c.Division != null ? c.Division.Nombre : string.Empty))
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Apellidos, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Correo, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Vendedor, op => op.AddTransform(y => y.ToUpper()));
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
                .ForMember(x => x.Comentarios, y => y.MapFrom(o => o.ConoceClienteOportunidad.Comentarios))
                .ForMember(x => x.Cuenta, y => y.MapFrom(o => o.CRMCliente.Nombre))
                .ForMember(x => x.Contacto, y => y.MapFrom(o => $"{o.Contacto.Nombre} {o.Contacto.Apellidos}"))
                .ForMember(x => x.Medida, y => y.MapFrom(o => o.UnidadMedida.Abreviacion))
                .ForMember(x => x.Equipo, y => y.MapFrom(o => o.Equipo.Nombre))
                .ForMember(x => x.Division, y => y.MapFrom(o => o.Equipo.Division.Nombre))
                .ForMember(x => x.Nombre_Opor, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Contacto, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Vendedor, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Equipo, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.TieneFormularioKYC, y => y.MapFrom(o => o.ConoceClienteOportunidad != null));

            CreateMap<CRMActividades, CRMActividadPostDTO>();
            CreateMap<CRMActividadDTO, CRMActividades>();
            CreateMap<CRMActividadPostDTO, CRMActividades>();
            CreateMap<CRMActividadDTO, CRMActividades>();
            CreateMap<CRMActividades, CRMActividadDTO>()
                .ForMember(x => x.Estatus, y => y.MapFrom(c => c.Estados.Valor))
                .ForMember(x => x.Asunto, y => y.MapFrom(c => c.Asuntos.Valor))
                .ForMember(x => x.Prioridad, y => y.MapFrom(c => c.Prioridades.Valor))
                .ForMember(x => x.Contacto_Rel, y => y.MapFrom(c => $"{c.Contacto.Nombre} {c.Contacto.Apellidos}"))
                .ForMember(x => x.Cuenta_Rel, y => y.MapFrom(c => c.Contacto.Cliente.Nombre ?? "No Asignado"))
                .ForMember(x => x.VendedorId, y => y.MapFrom(c => $"{c.Vendedor.Nombre} {c.Vendedor.Apellidos}"))
                .ForMember(x => x.Asunto, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Estatus, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Prioridad, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Contacto_Rel, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.VendedorId, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Desccripcion, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Retroalimentacion, op => op.AddTransform(y => y == null ? string.Empty : y.ToUpper()));

            CreateMap<ConoceClienteOportunidad, ConoceClienteOportunidadPostDTO>()
                .ForMember(x => x.DocumentoIdEtica1, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Etica1")
                     ? c.Documentos.First(d => d.Identificador == "Etica1").Id
                     : 0))
                .ForMember(x => x.DocumentoIdEtica2, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Etica2")
                     ? c.Documentos.First(d => d.Identificador == "Etica2").Id
                     : 0))
                .ForMember(x => x.DocumentoIdEtica3, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Etica3")
                     ? c.Documentos.First(d => d.Identificador == "Etica3").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional1, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional1")
                     ? c.Documentos.First(d => d.Identificador == "Adicional1").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional2, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional2")
                     ? c.Documentos.First(d => d.Identificador == "Adicional2").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional3, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional3")
                     ? c.Documentos.First(d => d.Identificador == "Adicional3").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional4, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional4")
                     ? c.Documentos.First(d => d.Identificador == "Adicional4").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional5, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional5")
                     ? c.Documentos.First(d => d.Identificador == "Adicional5").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional6, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional6")
                     ? c.Documentos.First(d => d.Identificador == "Adicional6").Id
                     : 0))
                .ForMember(x => x.DocumentoIdAdicional7, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional7")
                     ? c.Documentos.First(d => d.Identificador == "Adicional7").Id
                     : 0))
                  .ForMember(x => x.DocumentoIdAdicional8, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional8")
                     ? c.Documentos.First(d => d.Identificador == "Adicional8").Id
                     : 0))
                   .ForMember(x => x.DocumentoIdAdicional9, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional9")
                     ? c.Documentos.First(d => d.Identificador == "Adicional9").Id
                     : 0))
                        .ForMember(x => x.DocumentoIdAdicional10, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional10")
                     ? c.Documentos.First(d => d.Identificador == "Adicional10").Id
                     : 0))
                        .ForMember(x => x.DocumentoIdAdicional11, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Contrato")
                     ? c.Documentos.First(d => d.Identificador == "Contrato").Id
                     : 0))
                         .ForMember(x => x.DocumentoIdAdicional12, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "ContratoFinal")
                     ? c.Documentos.First(d => d.Identificador == "ContratoFinal").Id
                     : 0))
                         .ForMember(x => x.DocumentoIdAdicional13, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any(d => d.Identificador == "Adicional13")
                     ? c.Documentos.First(d => d.Identificador == "Adicional13").Id
                     : 0))
            .ForMember(x => x.NombreDocumento, y => y.MapFrom(c => c.Documentos != null && c.Documentos.Any()
                     ? c.Documentos.First().NombreDocumento
                     : string.Empty));


            CreateMap<ConoceClienteOportunidadPostDTO, ConoceClienteOportunidad>()
                 .ForMember(x => x.Documentos, op => op.Ignore()); ;
            CreateMap<ConoceClienteOportunidad, ConoceClienteOportunidad>();

            CreateMap<CRMVendedor, CRMVendedor>()
                .ForMember(x => x.UserId, o => o.Ignore());
            CreateMap<CRMVendedor, CRMVendedorDTO>()
                .ForMember(x => x.NombreDivision, y => y.MapFrom(c => c.Division.Nombre))
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Apellidos, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Correo, op => op.AddTransform(y => y.ToUpper()));
            CreateMap<CRMVendedorPostDTO, CRMVendedor>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMVendedor, CRMVendedorPostDTO>();
            CreateMap<CRMVendedor, CRMVendedorDetalleDTO>();
            CreateMap<CRMVendedorDTO, CRMVendedor>();

            CreateMap<CRMOriginador, CRMOriginador>()
                .ForMember(x => x.UserId, o => o.Ignore());
            CreateMap<CRMOriginador, CRMOriginadorDTO>()
                .ForMember(x => x.NombreDivision, y => y.MapFrom(c => c.Division.Nombre))
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Apellidos, op => op.AddTransform(y => y.ToUpper()))
                .ForMember(x => x.Correo, op => op.AddTransform(y => y.ToUpper()));
            CreateMap<CRMOriginadorPostDTO, CRMOriginador>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMOriginador, CRMOriginadorPostDTO>();
            CreateMap<CRMOriginador, CRMOriginadorDetalleDTO>();
            CreateMap<CRMOriginadorDTO, CRMOriginador>();

            CreateMap<CRMRol, CRMRolDTO>()
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()));
            CreateMap<CRMRol, CRMRolDetalleDTO>();
            CreateMap<CRMRol, CRMRolPostDTO>();
            CreateMap<CRMRolPostDTO, CRMRol>()
                .ForMember(x => x.Activo, op => op.Ignore());
            CreateMap<CRMEquipo, CRMEquipo>();
            CreateMap<CRMEquipo, CRMEquipoDTO>()
                .ForMember(x => x.Lideres, y => y.MapFrom(e => e.EquipoOriginadores.Select(x => x.Originador)))
                .ForMember(x => x.Division, y => y.MapFrom(e => e.Division.Nombre))
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()));
            CreateMap<CRMEquipo, CRMEquipoDetalleDTO>();
            CreateMap<CRMEquipo, CRMEquipoPostDTO>();
            CreateMap<CRMEquipoPostDTO, CRMEquipo>();

            CreateMap<CRMCliente, CRMClienteDTO>()
                .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()));
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


            CreateMap<CRMCatalogo, CRMCatalogoDTO>()
            .ForMember(x => x.Nombre, op => op.AddTransform(y => y.ToUpper()))
            .ForMember(x => x.Clave, op => op.AddTransform(y => y.ToUpper()));
            CreateMap<CRMCatalogoValor, CRMCatalogoValorDTO>();
            CreateMap<CRMCatalogoValor, CRMCatalogoValorPostDTO>();
            CreateMap<CRMCatalogoValorPostDTO, CRMCatalogoValor>();

            CreateMap<TipoDocumento, CRMTipoDocumentoDTO>()
                .ForMember(x => x.Nombre, y => y.MapFrom(c => c.Nombre))
                .ForMember(x => x.Activo, op => op.Ignore());
        }
    }
}
