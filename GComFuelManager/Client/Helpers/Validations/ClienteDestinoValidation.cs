using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class ClienteDestinoValidation : AbstractValidator<ClienteDestinoDTO>
    {
        public ClienteDestinoValidation()
        {
            RuleFor(x => x.Codcte)
                .NotNull().WithMessage("El cliente es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un cliente");
            RuleFor(x => x.Coddes)
                .NotNull().WithMessage("El destino es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un destino");
        }
    }
}

