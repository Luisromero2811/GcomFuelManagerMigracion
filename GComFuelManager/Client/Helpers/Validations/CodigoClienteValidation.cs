using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
    public class CodigoClienteValidation : AbstractValidator<CodCteDTO>
    {
        public CodigoClienteValidation()
        {
            RuleFor(x => x.cliente).NotEmpty().WithName("Nombre del Cliente");
            RuleFor(x => x.CodCte).NotEmpty().WithName("Código de Cliente");
        }
    }
}