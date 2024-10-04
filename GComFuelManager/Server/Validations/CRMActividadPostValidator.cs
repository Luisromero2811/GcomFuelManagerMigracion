﻿using System;
using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
{
	public class CRMActividadPostValidator : AbstractValidator<CRMActividadPostDTO>
	{
		public CRMActividadPostValidator()
		{
            RuleFor(x => x.Asunto).NotEmpty().WithName("Asunto");
            RuleFor(x => x.Prioridad).NotEmpty().WithName("Prioridad");
            RuleFor(x => x.Asignado).NotEmpty().WithName("Vendedor");
            RuleFor(x => x.Estatus).NotEmpty().WithName("Estado de la actividad");
            RuleFor(x => x.Contacto_Rel).NotEmpty().WithName("Contacto");
            RuleFor(x => x.EquipoId).NotEmpty().WithName("Equipo");
        }
	}
}

