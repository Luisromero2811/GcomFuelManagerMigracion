using FluentValidation;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class PreciosValidation:AbstractValidator<Precio>
    {
        public PreciosValidation()
        {
            RuleFor(x => x.codGru).NotEmpty().WithName("Grupo");
            RuleFor(x => x.codZona).NotEmpty().WithName("Zona");
            RuleFor(x => x.codCte).NotEmpty().WithName("Cliente");
            RuleFor(x => x.codDes).NotEmpty().WithName("Destino");
            RuleFor(x => x.codPrd).NotEmpty().WithName("Producto");
            RuleFor(x => x.Pre).NotEmpty().WithName("Precio");
        }
    }
}
