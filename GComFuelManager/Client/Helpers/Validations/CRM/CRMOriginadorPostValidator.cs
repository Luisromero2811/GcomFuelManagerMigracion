using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Client.Helpers.Validations.CRM
{
    public class CRMOriginadorPostValidator : AbstractValidator<CRMOriginadorPostDTO>
    {
        public CRMOriginadorPostValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre no puede estar vacío")
                .NotNull()
                .MaximumLength(100).WithMessage("El nombre no puede tener mas de 100 caracteres");

            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("Los apellidos no pueden estar vacíos")
                .NotNull()
                .MaximumLength(100).WithMessage("Los apellidos no pueden tener mas de 100 caracteres");

            RuleFor(x => x.Titulo)
                .MaximumLength(100).WithMessage("El titulo no puede tener mas de 100 caracteres");

            RuleFor(x => x.Departamento)
                .MaximumLength(100).WithMessage("El departamento no puede tener mas de 100 caracteres");

            RuleFor(x => x.Tel_Oficina)
                .MaximumLength(20).WithMessage("El teléfono de oficina no puede tener mas de 20 caracteres");

            RuleFor(x => x.Tel_Movil)
                .MaximumLength(20).WithMessage("El teléfono móvil no puede tener mas de 20 caracteres");

            RuleFor(x => x.Correo)
                .MaximumLength(100).WithMessage("El correo electrónico no puede tener mas de 100 caracteres");

            RuleFor(x => x.DivisionId)
                .NotEmpty().WithMessage("La división es obligatoria")
                .NotNull().WithMessage("La división es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una división");
        }
    }
}
