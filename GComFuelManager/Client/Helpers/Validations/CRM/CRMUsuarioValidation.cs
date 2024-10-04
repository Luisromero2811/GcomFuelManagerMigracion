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
			RuleFor(x => x.Password).NotEmpty().WithName("Contraseña");
			RuleFor(x => x.IDDivision).NotEmpty().WithName("División");
			//RuleFor(x => x.Roles).NotEmpty().WithName("Roles");
		}
	}
}

