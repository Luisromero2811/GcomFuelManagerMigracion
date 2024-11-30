using System.ComponentModel;

namespace GComFuelManager.Shared.ReportesDTO
{
    public class CatalogoChoferDTO
    {
        [DisplayName("Chofer")]
        public string Fullname { get; set; } = string.Empty;
    }
}
