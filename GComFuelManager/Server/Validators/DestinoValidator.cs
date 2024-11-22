using FluentValidation;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Server.Validators
{
    public class DestinoValidator : AbstractValidator<DestinoPostDTO>
    {
        public DestinoValidator()
        {
            RuleFor(x => x.Den)
                .MaximumLength(128).WithName("Nombre del destino").WithMessage("El nombre del destino no puede tener mas de 128 caracteres")
                .NotEmpty().WithName("Nombre del destino").WithMessage("El nombre del destino no puede estar vacio")
                .NotNull().WithName("Nombre del destino").WithMessage("El nombre del destino es obligatorio");
            RuleFor(x => x.Codcte)
                .NotNull()
                .WithMessage("El cliente es obligatorio")
                .GreaterThan(0)
                .WithMessage("Debe seleccionar un cliente");
            RuleFor(x => x.Dir)
                .MaximumLength(250)
                .WithMessage("La direccion no puede tener mas de 250 caracteres");
            RuleFor(x => x.Ciu)
                .MaximumLength(50)
                .WithMessage("La ciudad no puede tener mas de 50 caracteres");
            RuleFor(x => x.Est)
                .MaximumLength(50)
                .WithMessage("El estado no puede tener mas de 50 caracteres");
            RuleFor(x => x.Id_DestinoGobierno)
                .MaximumLength(20)
                .WithMessage("El id de gobierno no puede tener mas de 20 caracteres");
        }
    }
}
