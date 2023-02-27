using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GComFuelManager.Client;
using CurrieTechnologies.Razor.SweetAlert2;
using GComFuelManager.Client.Repositorios;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
ConfigureServices(builder.Services);



await builder.Build().RunAsync();
void ConfigureServices(IServiceCollection services)
{
    services.AddSweetAlert2();
    services.AddScoped<IRepositorio, Repositorio>();
}
