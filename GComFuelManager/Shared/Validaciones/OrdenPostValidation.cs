using FluentValidation;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Shared.Validaciones
{
    public class OrdenPostValidation : AbstractValidator<OrdenPostDTO>
    {
        public OrdenPostValidation()
        {
            RuleFor(x => x.DestinoId)
                .NotNull()
                .WithMessage("El destino es obligatorio")
                .GreaterThan(0)
                .WithMessage("Debe seleccionar un destino");

            RuleFor(x => x.ProductoId)
                .NotNull()
                .WithMessage("El destino es obligatorio")
                .GreaterThan((short)0)
                .WithMessage("Debe selecionar un producto");

            RuleFor(x => x.FechaCarga)
                .NotNull()
                .WithMessage("La fecha de carga es obligatoria")
                .NotEmpty()
                .WithMessage("La fecha de carga no puede estar vacia");

            RuleFor(x => x.FechaDeseadaCarga)
                .NotNull()
                .WithMessage("La fecha de recepción es obligatoria")
                .NotEmpty()
                .WithMessage("La fecha de recepción no puede estar vacia");

            RuleFor(x => x.Volumen)
                .NotNull()
                .WithMessage("El volumen es oblogatorio")
                .LessThanOrEqualTo(0)
                .WithMessage("El volumen no puede ser menor a 0");
        }
    }
}
