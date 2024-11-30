using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoTransportistaDTO
    {
        [DisplayName("Transportista")]
        public string Den { get; set; } = string.Empty;
        [DisplayName("Grupo trasportista")]
        public string GrupoTransportista { get; set; } = string.Empty;
    }
}
