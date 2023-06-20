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
			RuleFor(x => x.cliente).NotEmpty().WithName("Cliente");
			RuleFor(x => x.destino).NotEmpty().WithName("Destino");
		}
	}
}

