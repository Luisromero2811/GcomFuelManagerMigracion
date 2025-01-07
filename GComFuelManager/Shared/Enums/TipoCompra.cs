using System.ComponentModel;

namespace GComFuelManager.Shared.Enums
{
    //Pruebas para producción
    public enum TipoCompra
    {
        [Description("Rack")]
        Rack,
        [Description("Delivery")]
        Delivery,
        [Description("N/A")]
        NA
    }
}
