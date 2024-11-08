using AutoMapper;
using GComFuelManager.Shared.Extensiones;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;
using GComFuelManager.Shared.ReportesDTO;

namespace GComFuelManager.Server.Automapper
{
    public class MapperProfileReporte : Profile
    {
        public MapperProfileReporte()
        {
            CreateMap<Inventario, InventarioExcelDTO>()
                .ForMember(x => x.TipoInventario, opt => opt.MapFrom(y => y.TipoInventario.Description()));
            CreateMap<InventarioCierre, InventarioCierreExcelDTO>();
            CreateMap<InventarioCierreDTO, InventarioActualExcelDTO>();
        }
    }
}
