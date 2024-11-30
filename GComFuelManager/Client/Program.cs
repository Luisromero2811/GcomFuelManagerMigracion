using CurrieTechnologies.Razor.SweetAlert2;
using GComFuelManager.Client;
using GComFuelManager.Client.Auth;
using GComFuelManager.Client.Helpers;
using GComFuelManager.Client.Helpers.Validations;
using GComFuelManager.Client.Repositorios;
using GComFuelManager.Client.Validators;
using GComFuelManager.Shared.Modelos;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Radzen;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress),
    Timeout = TimeSpan.FromMinutes(15),
});
ConfigureServices(builder.Services);

builder.Logging.SetMinimumLevel(LogLevel.Information);

await builder.Build().RunAsync();
void ConfigureServices(IServiceCollection services)
{
    services.AddSweetAlert2();
    services.AddScoped<IRepositorio, Repositorio>();
    services.AddScoped<DialogService>();
    services.AddScoped<NotificationService>();
    services.AddScoped<TooltipService>();
    services.AddScoped<ContextMenuService>();

    services.AddAuthorizationCore();

    services.AddScoped<ProveedorAutenticacionJWT>();

    services.AddScoped<AuthenticationStateProvider, ProveedorAutenticacionJWT>(proveedor =>
    proveedor.GetRequiredService<ProveedorAutenticacionJWT>());

    services.AddScoped<ILoginService, ProveedorAutenticacionJWT>(proveedor =>
    proveedor.GetRequiredService<ProveedorAutenticacionJWT>());

    services.AddScoped<RenovadorToken>();
    services.AddSingleton(new CultureInfo("es-MX"));
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

    services.AddScoped<Constructor_De_URL_Parametros>();

}
