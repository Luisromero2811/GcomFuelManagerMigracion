using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
{
    public class CRMRolPostValidator : AbstractValidator<CRMRolPostDTO>
    {
        public CRMRolPostValidator()
        {
            RuleFor(x => x.Nombre)
                .NotNull().WithMessage("El nombre del rol es obligatorio")
                .NotEmpty().WithMessage("El nombre del rol no puede estar vacío");

            //RuleFor(x => x.Permisos)
            //    .NotNull().WithMessage("Los permisos son obligatorios")
            //    .Must(x => x.Count == 0).WithMessage("Debe seleccionar al menos un permiso");
        }
    }
}
