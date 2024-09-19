using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Client.Helpers.Validations.CRM
{
    public class CRMOportunidadPostValidator : AbstractValidator<CRMOportunidadPostDTO>
    {
        public CRMOportunidadPostValidator()
        {
            RuleFor(x => x.Nombre_Opor)
                .NotEmpty().WithMessage("El nombre de la oportunidad no puede estar vacio")
                .MaximumLength(200).WithMessage("El nombre de la oportunidad no puede ser mayor a 200 caracteres");
            RuleFor(x => x.ValorOportunidad)
                .NotEmpty().WithMessage("El valor de la oportunidad es obligatoria")
                .GreaterThanOrEqualTo(0).WithMessage("El valor de la oportunidad no puede ser menor a 0");
            RuleFor(x => x.UnidadMedidaId)
                .NotEmpty().WithMessage("La unidad de medida de la oportunidad es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida");
            RuleFor(x => x.TipoProductoId)
                .NotEmpty().WithMessage("El tipo de producto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo de producto");
            RuleFor(x => x.CantidadLts)
                .NotEmpty().WithMessage("La cantidad de la oportunidad es obligatoria")
                .GreaterThanOrEqualTo(0).WithMessage("La cantidad de la oportunidad no puede ser menor a 0");
            RuleFor(x => x.PrecioLts)
                .NotEmpty().WithMessage("El precio de la oportunidad es obligatorio")
                .GreaterThanOrEqualTo(0).WithMessage("El precio no puede ser menor a 0");
            RuleFor(x => x.TotalLts)
                .NotEmpty().WithMessage("El valor de la oportunidad es obligatoria")
                .GreaterThanOrEqualTo(0).WithMessage("El valor de la oportunidad no puede ser menor a 0");
            RuleFor(x => x.Prox_Paso)
                //.NotEmpty().WithMessage("La descripcion del proximo paso no puede estar vacia")
                .MaximumLength(200).WithMessage("La descripcion del proximo paso no puede ser mayor a 200 caracteres");
            RuleFor(x => x.OrigenPrductoId)
                .NotEmpty().WithMessage("El origen del producto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un origen de producto");
            RuleFor(x => x.VendedorId)
                .NotEmpty().WithMessage("El vendedor es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un vendedor");
            RuleFor(x => x.CuentaId)
                .NotEmpty().WithMessage("La cuenta es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una cuenta");
            RuleFor(x => x.ContactoId)
                .NotEmpty().WithMessage("El contacto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un contacto");
            RuleFor(x => x.TipoId)
                .NotEmpty().WithMessage("El tipo es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo");
            RuleFor(x => x.ModeloVentaId)
                .NotEmpty().WithMessage("El modleo de venta es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un modelo de venta");
            RuleFor(x => x.VolumenId)
                .NotEmpty().WithMessage("El volumen es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un volumen");
            RuleFor(x => x.FormaPagoId)
                .NotEmpty().WithMessage("La forma de pago es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una forma de pago");
            RuleFor(x => x.DiasPagoId)
                .NotEmpty().WithMessage("La forma de pago es obligatoria")
                .GreaterThan(0)
                .When(x => x.FormaPagoId == 95)
                .WithMessage("Debe seleccionar un dia de pago");
            //RuleFor(x => x.FechaCierre)
            //    .GreaterThanOrEqualTo(x => x.FechaCierre).WithMessage($"La fecha de cierre no puede ser menor a {DateTime.Today.ToShortDateString()}");
            RuleFor(x => x.EtapaVentaId)
                .NotEmpty().WithMessage("La etapa de venta es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una etapa de venta");
            RuleFor(x => x.Probabilidad)
                .GreaterThanOrEqualTo(0).WithMessage("La probabilidad no puede ser menor a 0")
                .LessThanOrEqualTo(100).WithMessage("La probabilidad no puede ser mayor a 100");
        }
    }
}
