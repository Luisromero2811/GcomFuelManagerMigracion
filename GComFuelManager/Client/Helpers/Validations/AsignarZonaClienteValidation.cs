using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
	public class AsignarZonaClienteValidation : AbstractValidator<ZonaDTO>
	{
		public AsignarZonaClienteValidation()
		{
			RuleFor(x => x.ZonaCod).NotEmpty().WithName("Zona");
			RuleFor(x => x.codgru).NotEmpty().WithName("Grupo");
			RuleFor(x => x.CteCod).NotEmpty().WithName("Cliente");
			RuleFor(x => x.DesCod).NotEmpty().WithName("Destino");
		}
	}
}

