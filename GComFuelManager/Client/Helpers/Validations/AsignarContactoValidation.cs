using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs;
using GComFuelManager.Shared.Modelos;

namespace GComFuelManager.Client.Helpers.Validations
{
	public class AsignarContactoValidation : AbstractValidator<Contacto>
	{
		public AsignarContactoValidation()
		{
			RuleFor(x => x.Nombre).NotEmpty().WithName("Nombre");
			RuleFor(x => x.Correo).NotEmpty().WithName("Correo");
		}
	}
}

