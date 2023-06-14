using FluentValidation;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class OrdenCierreClientValidation:AbstractValidator<OrdenCierre>
    {
        public OrdenCierreClientValidation()
        {
            RuleFor(o => o.CodDes).NotEmpty().WithName("Destino");
            RuleFor(o => o.TipoPago).NotEmpty().WithName("Tipo de pago");
            RuleFor(o => o.CodPrd).NotEmpty().WithName("Producto");
        }
    }
}
