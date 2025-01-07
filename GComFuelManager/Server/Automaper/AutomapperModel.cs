using AutoMapper;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.ModelDTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Server.Automaper
{
    public class AutomapperModel : Profile
    {
        public AutomapperModel()
        {
            CreateMap<Destino, DestinoDTO>();

            CreateMap<Producto, ProductoDTO>();
        }
    }
}
