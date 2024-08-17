using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Shared.Modelos
{
    [Keyless]
    public class CRMHistorialEstadosCliente
    {
        public int Cliente { get; set; }
        public int Estado { get; set; }
        public DateTime Fecha_Registro { get; set; }
        public int Usuario_Mod { get; set; }
    }
}
