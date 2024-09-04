using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Client.Helpers.Validations.CRM
{
    public class CRMEquipoPostValidator : AbstractValidator<CRMEquipoPostDTO>
    {
        public CRMEquipoPostValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre no puede estar vacío")
                .NotNull()
                .MaximumLength(100).WithMessage("El nombre no puede tener mas de 100 caracteres");

            RuleFor(x => x.LiderId)
                .NotEmpty().WithMessage("El líder de equipo es obligatorio")
                .NotNull().WithMessage("El líder de equipo es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un líder de equipo");

            RuleFor(x => x.DivisionId)
                .NotEmpty().WithMessage("La división es obligatoria")
                .NotNull().WithMessage("La división es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una división");

            RuleFor(x => x.Vendedores)
                .NotNull().WithMessage("Los permisos son obligatorios")
                .Must(x => x.Count <= 0).WithMessage("Debe seleccionar al menos un integrante");
        }
    }
}
