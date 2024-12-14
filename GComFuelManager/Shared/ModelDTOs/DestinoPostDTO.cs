using GComFuelManager.Shared.Enums;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class DestinoPostDTO
    {
        public int Cod { get; set; }
        public string Den { get; set; } = string.Empty;
        public int Codcte { get; set; }
        public long CodGamo { get; set; }
        public string? Id_DestinoGobierno { get; set; } = string.Empty;
        public string? Dir { get; set; } = string.Empty;
        public string? Ciu { get; set; } = string.Empty;
        public string? Est { get; set; } = string.Empty;
        public bool Es_Multidestino { get; set; } = false;
        public ModeloVenta TipoVenta { get; set; }
        public TipoVenta ModeloVenta { get; set; }
        public TipoCompra ModeloCompra { get; set; }
    }
}
