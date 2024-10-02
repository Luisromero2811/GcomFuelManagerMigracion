using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
{
    public class CRMCatalgoValorPostValidator : AbstractValidator<CRMCatalogoValorPostDTO>
    {
        public CRMCatalgoValorPostValidator()
        {
            RuleFor(x => x.Valor)
                .MaximumLength(100).WithMessage("El valor no puede tener mas de 100 caracteres")
                .NotEmpty().WithMessage("El valor no puede estar vacio")
                .NotNull().WithMessage("El valor es obligatorio");

            RuleFor(x => x.Valor)
                .MaximumLength(100).WithMessage("La abreviacion no puede tener mas de 100 caracteres");
        }
    }
}
