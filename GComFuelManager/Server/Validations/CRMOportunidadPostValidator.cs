using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
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
                .GreaterThan(0).WithMessage("El valor de la oportunidad no puede ser menor a 0");
            RuleFor(x => x.UnidadMedidaId)
                .NotEmpty().WithMessage("La unidad de medida de la oportunidad es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una unidad de medida");
            RuleFor(x => x.Prox_Paso)
                //.NotEmpty().WithMessage("La descripcion del proximo paso no puede estar vacia")
                .MaximumLength(200).WithMessage("La descripcion del proximo paso no puede ser mayor a 200 caracteres");
            RuleFor(x => x.OrigenId)
                .NotEmpty().WithMessage("El origen es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un origen");
            RuleFor(x => x.VendedorId)
                .NotEmpty().WithMessage("El vendedor es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un vendedor");
            //RuleFor(x => x.CuentaId)
            //    .NotEmpty().WithMessage("La cuenta es obligatoria")
            //    .GreaterThan(0).WithMessage("Debe seleccionar una cuenta");
            RuleFor(x => x.TipoId)
                .NotEmpty().WithMessage("El tipo es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un tipo");
            RuleFor(x => x.FechaCierre)
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage($"La fecha de cierre no puede ser menor a {DateTime.Today.ToShortDateString()}");
            RuleFor(x => x.EtapaVentaId)
                .NotEmpty().WithMessage("La etapa de venta es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una etapa de venta");
            RuleFor(x => x.Probabilidad)
                .GreaterThanOrEqualTo(0).WithMessage("La probabilidad no puede ser menor a 0")
                .LessThanOrEqualTo(100).WithMessage("La probabilidad no puede ser mayor a 100");
        }
    }
}
