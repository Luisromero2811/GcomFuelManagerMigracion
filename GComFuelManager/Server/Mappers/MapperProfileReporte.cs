using AutoMapper;
using GComFuelManager.Shared.DTOs.Reportes.CRM;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Mappers
{
    public class MapperProfileReporte : Profile
    {
        public MapperProfileReporte()
        {
            CreateMap<CRMOportunidad, CRMOportunidadExcelDTO>()
                .ForMember(x => x.Oportunidad, o => o.MapFrom(x => x.Nombre_Opor))
                .ForMember(x => x.OrigenProducto, o => o.MapFrom(x => x.OrigenProducto.Valor))
                .ForMember(x => x.TipoProducto, o => o.MapFrom(x => x.TipoProducto.Valor))
                .ForMember(x => x.ValorOportunidad, o => o.MapFrom(x => x.ValorOportunidad))
                .ForMember(x => x.Vendedor, o => o.MapFrom(x => x.Vendedor.FullName))
                .ForMember(x => x.Division, o => o.MapFrom(x => x.Equipo.Division.Nombre))
                .ForMember(x => x.Equipo, o => o.MapFrom(x => x.Equipo.Nombre))
                .ForMember(x => x.Cuenta, o => o.MapFrom(x => x.CRMCliente.Nombre))
                .ForMember(x => x.Contacto, o => o.MapFrom(x => $"{x.Contacto.Nombre} {x.Contacto.Apellidos}"))
                .ForMember(x => x.Tel_Movil, o => o.MapFrom(x => x.Contacto.Tel_Movil))
                .ForMember(x => x.Tel_Oficina, o => o.MapFrom(x => x.Contacto.Tel_Oficina))
                .ForMember(x => x.Correo, o => o.MapFrom(x => x.Contacto.Correo))
                .ForMember(x => x.UnidadMedida, o => o.MapFrom(x => x.UnidadMedida.Valor))
                .ForMember(x => x.CantidadOportuniad, o => o.MapFrom(x => x.CantidadLts))
                .ForMember(x => x.Precio, o => o.MapFrom(x => x.PrecioLts))
                .ForMember(x => x.TotalLts, o => o.MapFrom(x => x.TotalLts))
                .ForMember(x => x.Periodo, o => o.MapFrom(x => x.Periodo.Valor))
                .ForMember(x => x.RelacionComercial, o => o.MapFrom(x => x.Tipo.Valor))
                .ForMember(x => x.ModeloVenta, o => o.MapFrom(x => x.ModeloVenta.Valor))
                .ForMember(x => x.Volumen, o => o.MapFrom(x => x.Volumen.Valor))
                .ForMember(x => x.FormaPago, o => o.MapFrom(x => x.FormaPago.Valor))
                .ForMember(x => x.DiasPago, o => o.MapFrom(x => x.DiasCredito != null ? x.DiasCredito.Valor : string.Empty))
                .ForMember(x => x.CantidadEstaciones, o => o.MapFrom(x => x.CantidadEstaciones))
                .ForMember(x => x.EtapaVenta, o => o.MapFrom(x => x.EtapaVenta.Valor))
                .ForMember(x => x.ProximoPaso, o => o.MapFrom(x => x.Prox_Paso));

            CreateMap<CRMActividades, CRMActividadesExcelDTO>()
                .ForMember(x => x.Contacto_Rel, o => o.MapFrom(x => x.contacto.Nombre))
                .ForMember(x => x.Asunto, o => o.MapFrom(x => x.asuntos.Valor))
                .ForMember(x => x.Prioridad, o => o.MapFrom(x => x.prioridades.Valor))
                .ForMember(x => x.Fch_Inicio, o => o.MapFrom(x => x.Fch_Inicio))
                .ForMember(x => x.Fecha_Ven, o => o.MapFrom(x => x.Fecha_Ven))
                .ForMember(x => x.Estatus, o => o.MapFrom(x => x.Estados.Valor))
                .ForMember(x => x.Asignado, o => o.MapFrom(x => $"{x.vendedor.Nombre} {x.vendedor.Apellidos}"));

            CreateMap<CRMContacto, CRMContactosExcelDTO>()
                .ForMember(x => x.Nombre, o => o.MapFrom(x => $"{x.Nombre} {x.Apellidos}"))
                .ForMember(x => x.Cuenta, o => o.MapFrom(x => x.Cliente.Nombre))
                .ForMember(x => x.Tel_Movil, o => o.MapFrom(x => x.Tel_Movil.ToString()))
                .ForMember(x => x.Tel_Oficina, o => o.MapFrom(x => x.Tel_Oficina.ToString()))
                .ForMember(x => x.Correo, o => o.MapFrom(x => x.Correo))
                .ForMember(x => x.Vendedor, o => o.MapFrom(x => $"{x.Vendedor.Nombre} {x.Vendedor.Apellidos}"))
                .ForMember(x => x.Division, o => o.MapFrom(x => x.Division.Nombre))
                .ForMember(x => x.Estado, o => o.MapFrom(x => x.Estatus.Valor))
                .ForMember(x => x.Titulo, o => o.MapFrom(x => x.Titulo))
                .ForMember(x => x.Departamento, o => o.MapFrom(x => x.Departamento))
                .ForMember(x => x.Calle, o => o.MapFrom(x => x.Calle))
                .ForMember(x => x.Colonia, o => o.MapFrom(x => x.Colonia))
                .ForMember(x => x.Ciudad, o => o.MapFrom(x => x.Ciudad))
                .ForMember(x => x.CP, o => o.MapFrom(x => x.CP))
                .ForMember(x => x.SitioWeb, o => o.MapFrom(x => x.SitioWeb))
                .ForMember(x => x.Recomen, o => o.MapFrom(x => x.Recomen));
        }
    }
}
