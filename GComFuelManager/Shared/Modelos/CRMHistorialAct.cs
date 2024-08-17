using Microsoft.EntityFrameworkCore;

namespace GComFuelManager.Shared.Modelos
{
    [Keyless]
    public class CRMHistorialAct
    {
        public int Actividad { get; set; }
        public int Estatus { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public DateTime Fecha_Mod { get; set; }
        public DateTime Fecha_Ven { get; set; }
        public int Asignado { get; set; }
        public bool Activo { get; set; }
    }
}
