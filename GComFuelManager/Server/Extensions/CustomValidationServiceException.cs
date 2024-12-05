using FluentValidation;
using GComFuelManager.Server.Validators;
using GComFuelManager.Shared.ModelDTOs;

namespace GComFuelManager.Server.Extensions
{
    public static class CustomValidationServiceException
    {
        public static IServiceCollection AddCustomValidationService(this IServiceCollection services)
        {
            services.AddScoped<IValidator<InventarioPostDTO>, InventarioValidator>();
            services.AddScoped<IValidator<TerminalPostDTO>, TerminalValidator>();
            services.AddScoped<IValidator<CatalogoValorPostDTO>, CatalogoValorValidator>();
            services.AddScoped<IValidator<DestinoPostDTO>, DestinoValidator>();
            return services;
        }
    }
}
