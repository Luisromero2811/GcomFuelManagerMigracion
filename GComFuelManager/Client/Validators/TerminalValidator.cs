using FluentValidation;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Client.Validators
{
    public class TerminalValidator : AbstractValidator<TerminalPostDTO>
    {
        public TerminalValidator()
        {
            RuleFor(x => x.Den)
                .NotNull().WithMessage("El nombre de la unidad de negocio es obligatorio")
                .NotEmpty().WithMessage("El nombre de la unidad de negocio no puede estar vacio")
                .MaximumLength(100).WithMessage("El nombre de la unidad de negocio no puede terner mas de 100 caracteres");

            RuleFor(x => x.Codigo)
                .NotNull().WithMessage("La abreviacion de la unidad de negocio es obligatoria")
                .NotEmpty().WithMessage("La abreviacion de la unidad de negocio no puede estar vacia")
                .MaximumLength(100).WithMessage("La abreviacion de la unidad de negocio no puede terner mas de 5 caracteres");

            RuleFor(x => x.CodigoOrdenes)
                .NotNull().WithMessage("El identificador de orden de la unidad de negocio es obligatorio")
                .NotEmpty().WithMessage("El identificador de orden de la unidad de negocio no puede estar vacio")
                .MaximumLength(100).WithMessage("El identificador de orden de la unidad de negocio no puede terner mas de 5 caracteres");

            RuleFor(x => x.TipoTerminalId)
                .NotNull().WithMessage("El tipo de de unidad de negocio es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de unidad de negocio");
        }
    }
}
