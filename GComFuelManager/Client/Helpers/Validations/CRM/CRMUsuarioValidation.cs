using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Client.Helpers.Validations.CRM
{
	public class CRMUsuarioValidation : AbstractValidator<CRMUsuarioDTO>
	{
		public CRMUsuarioValidation()
		{
			RuleFor(x => x.UserName).NotEmpty().WithName("Nombre");
		}
	}
}

