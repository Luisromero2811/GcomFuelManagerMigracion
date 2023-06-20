using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
	public class AsignacionZonaValidation : AbstractValidator<Zona>
	{
		public AsignacionZonaValidation()
		{
			RuleFor(x => x.Nombre).NotEmpty().WithName("Zona");
		}
	}
}

