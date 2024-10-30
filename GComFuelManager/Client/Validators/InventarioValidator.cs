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

            RuleFor(x => x.UnidadMedidaId)
                .NotNull().WithMessage("La unidad de medida es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida");

            //RuleFor(x => x.TransportistaId)
            //    .NotNull().WithMessage("El transportista es obligatorio")
            //    .GreaterThan(0).WithMessage("Debe seleccionar un transportista");

            RuleFor(x => x.TonelId)
                .NotNull().WithMessage("El vehiculo es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un vehiculo");

            //RuleFor(x => x.GrupoId)
            //    .NotNull().WithMessage("El grupo es obligatorio")
            //    .GreaterThan((short)0).WithMessage("Debe seleccionar un grupo");

            RuleFor(x => x.ClienteId)
                .NotNull().WithMessage("El cliente es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un cliente");

            RuleFor(x => x.Numero)
                .MaximumLength(50).WithMessage("El numero no puede tener mas de 50 caracteres");

            RuleFor(x => x.Cantidad)
                .NotNull().WithMessage("La cantidad es obligatoria");
            //.GreaterThan(0)
            //.When(x => new List<int>() { 58, 59, 60, 61, 64 }.Contains(x.TipoMovimientoId))
            //.WithMessage("La cantidad no puede ser menor a 0")
            //.LessThan(0)
            //.When(x => new List<int>() { 66, 67, 68 }.Contains(x.TipoMovimientoId))
            //.WithMessage("La cantidad no puede ser mayor a 0");
            //.GreaterThanOrEqualTo(0).WithMessage("La cantidad no puede ser menor a 0");

            RuleFor(x => x.Numero)
                .NotNull().WithMessage("El numero es obligatoria")
                .NotEmpty().WithMessage("El numero no puede estar vacio");
        }
    }
}
