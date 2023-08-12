using FluentValidation;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class AsignarContactoClienteValidation : AbstractValidator<Contacto>
    {
        public AsignarContactoClienteValidation()
        {
            RuleFor(x => x.Nombre).NotEmpty().WithName("Nombre").Length(26);
            RuleFor(x => x.Correo).NotEmpty().WithName("Correo").Length(70).EmailAddress();
        }
    }
}
