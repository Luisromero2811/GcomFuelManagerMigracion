using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class AsignacionUnidadValidation: AbstractValidator<AsignacionDTO>
    {
        public AsignacionUnidadValidation()
        {
            RuleFor(oe => oe.CodChf).NotEmpty().WithName("Chofer");
            RuleFor(oe => oe.CodTon).NotEmpty().WithName("Unidad");
            RuleFor(oe => oe.CodTra).NotEmpty().WithName("Transportista");
        }
    }
}
