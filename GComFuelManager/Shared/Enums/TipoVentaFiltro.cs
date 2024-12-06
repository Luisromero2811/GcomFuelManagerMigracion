using System.ComponentModel;

namespace GComFuelManager.Shared.Enums
{
    public enum TipoVentaFiltro
    {
        [Description("Ambos delivery")]
        AmbosDelivery,
        [Description("Interno")]
        Interno,
        [Description("Externo")]
        Externo,
        [Description("Rack")]
        Rack,
        [Description("Todos")]
        Todos
    }
}
