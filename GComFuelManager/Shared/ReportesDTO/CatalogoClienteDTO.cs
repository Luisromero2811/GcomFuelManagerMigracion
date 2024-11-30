using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoClienteDTO
    {
        [DisplayName("Cliente")]
        public string Den { get; set; } = string.Empty;
        public string Grupo { get; set; } = string.Empty;
        [DisplayName("Tipo de venta")]
        public string Tipven { get; set; } = string.Empty;
        [DisplayName("Modelo de venta")]
        public string MdVenta { get; set; } = string.Empty;
        [DisplayName("Identificador externo")]
        public string Identificador_Externo { get; set; } = string.Empty;
    }
}
