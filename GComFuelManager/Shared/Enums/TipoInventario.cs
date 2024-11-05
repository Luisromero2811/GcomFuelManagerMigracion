using System.ComponentModel;

namespace GComFuelManager.Shared.Enums;
public enum TipoInventario
{
    [Description("Inventario")]
    Inicial,
    [Description("Fisico Reservado")]
    FisicaReservada,
    [Description("Orden Reservada")]
    OrdenReservada,
    [Description("En Orden")]
    EnOrden
}

