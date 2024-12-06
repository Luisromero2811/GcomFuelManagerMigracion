using FluentValidation;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Client.Validators
{
    public class InventarioValidator : AbstractValidator<InventarioPostDTO>
    {
        public InventarioValidator()
        {
            RuleFor(x => x.ProductoId)
                .NotNull().WithMessage("El producto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un producto");

            RuleFor(x => x.SitioId)
                .NotNull().WithMessage("El sitio es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un sitio");

            RuleFor(x => x.AlmacenId)
                .NotNull().WithMessage("El almacen es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un almacen");

            RuleFor(x => x.LocalidadId)
                .NotNull().WithMessage("La localidad es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar una localidad");

            RuleFor(x => x.TipoMovimientoId)
                .NotNull().WithMessage("El tipo de moviento es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de moviento");

            //RuleFor(x => x.UnidadMedidaId)
            //    .NotNull().WithMessage("La unidad de medida es obligatoria")
            //    .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida");

            RuleFor(x => x.OrigenDestinoID)
                .NotNull().WithMessage("El destino/origen es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un destino/origen");

            RuleFor(x => x.Numero)
                .MaximumLength(50).WithMessage("El numero no puede tener mas de 50 caracteres");

            //RuleFor(x => x.Cantidad)
            //    .NotNull().WithMessage("La cantidad es obligatoria")
            //    .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser menor a 0");

            RuleFor(x => x.TirillaInicial)
                .NotNull().WithMessage("La cantidad es obligatoria")
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser menor a 0");

            RuleFor(x => x.TirillaFinal)
                .NotNull().WithMessage("La cantidad es obligatoria")
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser menor a 0");

            //RuleFor(x => x.Numero)
            //    .NotNull().WithMessage("El numero es obligatoria")
            //    .NotEmpty().WithMessage("El numero no puede estar vacio");
        }
    }
}
