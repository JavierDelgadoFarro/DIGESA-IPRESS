using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VisitasTickets.Client.Services;
using VisitasTickets.Client.Globals;

namespace VisitasTickets.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri($"{AppConfig.ApiBaseUrl}/")
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            builder.Services.AddScoped<UsuarioService>();
            builder.Services.AddScoped<AtencionService>();
            builder.Services.AddScoped<VisitanteService>();
            builder.Services.AddSingleton<NotificationService>();
            builder.Services.AddScoped<SignalRService>();
            builder.Services.AddScoped<VisitanteSignalRService>();

            await builder.Build().RunAsync();
        }
    }
}