using FluentValidation;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Server.Validators
{
    public class CatalogoValorValidator : AbstractValidator<CatalogoValorPostDTO>
    {
        public CatalogoValorValidator()
        {
            RuleFor(x => x.Valor)
                .NotNull().WithMessage("El valor es obligatorio")
                .NotEmpty().WithMessage("El valor no puede estar vacio")
                .MaximumLength(150).WithMessage("El valor no puede tener mas de 150 caracteres");

            RuleFor(x => x.Abreviacion)
                .MaximumLength(50).WithMessage("La abreviacion no puede tener mas de 50 caracteres");

            RuleFor(x => x.CatalogoId)
                .NotNull().WithMessage("El catalogo es obligatorio")
                .NotEmpty().WithMessage("El catalogo no puede estar vacio")
                .GreaterThan(0).WithMessage("Debe seleccionar un catalogo");
        }
    }
}
