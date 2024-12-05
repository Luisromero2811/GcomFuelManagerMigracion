using RazorHtmlEmails.Common;
using RazorHtmlEmails.GComFuelManagerMigracion.Services;

namespace GComFuelManager.Server.Extensions
{
    public static class CustomEmailSendServiceExtension
    {
        public static IServiceCollection AddCustomEmailService(this IServiceCollection services)
        {
            services.AddScoped<IRazorViewToStringRenderer, RazorViewToStringRender>();
            services.AddScoped<IRegisterAccountService, RegisterAccountService>();
            services.AddScoped<IVencimientoService, VencimientoEmailService>();
            services.AddScoped<IPreciosService, PreciosService>();
            services.AddScoped<IConfirmOrden, ConfirmOrden>();
            services.AddScoped<IConfirmarCreacionOrdenes, ConfirmarCreacionOrdenesService>();
            services.AddScoped<IDenegarCreacionOrdenes, DenegarCreacionOrdenesService>();
            services.AddScoped<IConfirmPedido, ConfirmPedido>();

            return services;
        }
    }
}
