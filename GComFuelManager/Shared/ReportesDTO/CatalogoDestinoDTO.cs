using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoDestinoDTO
    {
        public string Den { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        [DisplayName("Codigo cliente")]
        public string? Codsyn { get; set; } = string.Empty;
        [DisplayName("Codigo gamo")]
        public long? CodGamo { get; set; }
        [DisplayName("Codigo de gobierno")]
        public string? Id_DestinoGobierno { get; set; } = string.Empty;

        [DisplayName("Dirección")]
        public string? Dir { get; set; } = string.Empty;

        [DisplayName("Ciudad")]
        public string? Ciu { get; set; } = string.Empty;
        [DisplayName("Estado")]
        public string? Est { get; set; } = string.Empty;
        [DisplayName("Nombre completo del destino")]
        public string FULLDEN { get; set; } = string.Empty;
    }
}
