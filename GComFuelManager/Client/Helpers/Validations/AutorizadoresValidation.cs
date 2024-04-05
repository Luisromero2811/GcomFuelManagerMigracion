using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
	public class AutorizadoresValidation : AbstractValidator<Autorizador>
	{
		public AutorizadoresValidation()
		{
			RuleFor(x => x.Den).NotEmpty().WithName("Nombre del Autorizador");
		}
	}
}

