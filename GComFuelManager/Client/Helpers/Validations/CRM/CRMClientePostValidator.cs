using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Client.Helpers.Validations.CRM
{
    public class CRMClientePostValidator : AbstractValidator<CRMClientePostDTO>
    {
        public CRMClientePostValidator()
        {
            RuleFor(x => x.Nombre)
                .NotNull()
                .WithMessage("El nombre de la cuenta es obligatorio")
                .NotEmpty()
                .WithMessage("El nombre de la cuenta no puede estar vacio");
        }
    }
}
