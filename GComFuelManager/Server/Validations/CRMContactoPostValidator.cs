using FluentValidation;
using GComFuelManager.Shared.DTOs.CRM;

namespace GComFuelManager.Server.Validations
{
    public class CRMContactoPostValidator : AbstractValidator<CRMContactoPostDTO>
    {
        public CRMContactoPostValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre es obligatorio")
                .MaximumLength(100).WithMessage("El nombre no debe tener mas de 100 caracteres");
            RuleFor(x => x.Apellidos)
                .NotEmpty().WithMessage("El apellido es obligatorio")
                .MaximumLength(100).WithMessage("Los apellidos no deben tener mas de 100 caracteres");
            RuleFor(x => x.Titulo)
                .MaximumLength(100).WithMessage("El titulo no debe tener mas de 100 caracteres");
            RuleFor(x => x.Departamento)
                .MaximumLength(100).WithMessage("El departamento no debe tener mas de 100 caracteres");
            RuleFor(x => x.Tel_Oficina)
                .MaximumLength(20).WithMessage("El telefono de oficina no debe tener mas de 20 caracteres");
            RuleFor(x => x.Tel_Movil)
                .MaximumLength(20).WithMessage("El telefono movil no debe tener mas de 20 caracteres");
            RuleFor(x => x.SitioWeb)
                .MaximumLength(200).WithMessage("La direccion del sitio web no debe tener mas de 200 caracteres");
            RuleFor(x => x.Correo)
                .MaximumLength(100).WithMessage("El correo no debe tener mas de 100 caracteres");
            RuleFor(x => x.Calle)
                .MaximumLength(200).WithMessage("La calle no debe tener mas de 100 caracteres");
            RuleFor(x => x.Colonia)
                .MaximumLength(50).WithMessage("La colonia no debe tener mas de 50 caracteres");
            RuleFor(x => x.Ciudad)
                .MaximumLength(50).WithMessage("La ciudad no debe tener mas de 50 caracteres");
            RuleFor(x => x.CP)
                .MaximumLength(200).WithMessage("El codigo postal no debe tener mas de 10 caracteres");
            RuleFor(x => x.Pais)
                .MaximumLength(200).WithMessage("El país no debe tener mas de 50 caracteres");
            RuleFor(x => x.SitioWeb)
                .MaximumLength(200).WithMessage("La direccion del sitio web no debe tener mas de 200 caracteres");
            RuleFor(x => x.EstatusId)
                .NotEmpty().WithMessage("El estado es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un estado de contacto");
            RuleFor(x => x.Estatus_Desc)
                .MaximumLength(250).WithMessage("La descripcion del estado no debe tener mas de 250 caracteres");
            RuleFor(x => x.OrigenId)
                .NotEmpty().WithMessage("El origen del contacto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un origen de contacto");
            RuleFor(x => x.Recomen)
                .MaximumLength(250).WithMessage("La descripcion del estado no debe tener mas de 200 caracteres");
            RuleFor(x => x.VendedorId)
                .NotEmpty().WithMessage("El encargado del contacto es obligatorio")
                .GreaterThan(0).WithMessage("Debe seleccionar un encargado del contacto");
            RuleFor(x => x.CuentaId)
                .NotEmpty().WithMessage("La cuenta es obligatoria")
                .GreaterThan(0).WithMessage("Debe seleccionar una cuenta de contacto");
        }
    }
}
