using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
{
	public class CRMActividadPostValidator : AbstractValidator<CRMActividadPostDTO>
	{
		public CRMActividadPostValidator()
		{
			RuleFor(x => x.Asunto)
				.NotEmpty().WithMessage("El asunto debe ser obligatorio");
			RuleFor(x => x.Prioridad)
				.NotEmpty().WithMessage("La prioridad debe ser obligatoria");
		}
	}
}

