using System;
using FluentValidation;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
	public class GestionChoferesValidation : AbstractValidator<Chofer>
	{
		public GestionChoferesValidation()
		{
			RuleFor(x => x.Id_Transportista).NotEmpty().WithName("Empresa Transportista");
			RuleFor(x => x.Den).NotEmpty().WithName("Nombres del Chofer");
			RuleFor(x => x.Shortden).NotEmpty().WithName("Apellidos del Chofer");
			//RuleFor(x => x.RFC).NotEmpty().WithName("RFC");
		}
	}
}

