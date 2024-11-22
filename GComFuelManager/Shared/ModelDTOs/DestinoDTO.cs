using GComFuelManager.Shared.Enums;
using System.ComponentModel;

namespace GComFuelManager.Shared.ModelDTOs
{
    public class DestinoDTO
    {
        public int Cod { get; set; }
        public string Den { get; set; } = string.Empty;
        public string Dir { get; set; } = string.Empty;
        public string Ciu { get; set; } = string.Empty;
        public string Est { get; set; } = string.Empty;
        public short Id_Tad { get; set; } = 0;
        public string Codsyn { get; set; } = string.Empty;
        public long CodGamo { get; set; } = 0;
        public int Codcte { get; set; } = 0;
        [DisplayName("ID Gobierno")]
        public string Id_DestinoGobierno { get; set; } = string.Empty;
        public Tipo_Venta? TipoVenta { get; set; }
        public Modelo_Venta? ModeloVenta { get; set; }
        public bool Activo { get; set; } = false;
    }
}
