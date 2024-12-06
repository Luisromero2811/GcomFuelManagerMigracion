using GComFuelManager.Client.Helpers.Validations;
using GComFuelManager.Client.Validators;

namespace GComFuelManager.Client.Extensions
{
    public static class CustomValidationsServiceExtension
    {
        public static IServiceCollection AddCustomValidations(this IServiceCollection services)
        {
            services.AddScoped<OrdenCierreAdminValidation>();
            services.AddScoped<OrdenCierreClientValidation>();
            services.AddScoped<PedidoOrdenValidation>();
            services.AddScoped<AsignacionUnidadValidation>();
            services.AddScoped<UsuarioInfoValidation>();
            services.AddScoped<CodigoClienteValidation>();
            services.AddScoped<ContactoInternoValidation>();
            services.AddScoped<AsignarGrupoClienteValidation>();
            services.AddScoped<AsignarGrupoValidation>();
            services.AddScoped<AsignacionZonaValidation>();
            services.AddScoped<AsignarContactoValidation>();
            services.AddScoped<AsignarZonaClienteValidation>();
            services.AddScoped<ClienteDestinoValidation>();
            services.AddScoped<AsignarContactoClienteValidation>();
            services.AddScoped<CierreGrupoValidation>();
            services.AddScoped<PreciosValidation>();
            services.AddScoped<GestionClienteValidation>();
            services.AddScoped<GestionDestinoValidation>();
            services.AddScoped<GestionGrupoTransportesValidation>();
            services.AddScoped<GestionEmpresaTransportesValidation>();
            services.AddScoped<GestionChoferesValidation>();
            services.AddScoped<GestionUnidadValidation>();
            services.AddScoped<AutorizadoresValidation>();

            services.AddScoped<InventarioValidator>();
            services.AddScoped<TerminalValidator>();
            services.AddScoped<CatalogoValorValidator>();

            return services;
        }
    }
}
