using System.ComponentModel;

namespace GComFuelManager.Shared.Enums
{
    public enum TipoVentaFiltro
    {
        [Description("Interno")]
        Interno,
        [Description("Externo")]
        Externo,
        [Description("Rack")]
        Rack,
        [Description("Ambos delivery")]
        AmbosDelivery,
        [Description("Todos")]
        Todos
    }
}
