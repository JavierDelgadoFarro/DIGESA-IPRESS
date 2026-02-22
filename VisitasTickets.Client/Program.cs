using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using VisitasTickets.Client.Services;

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
                BaseAddress = new Uri("https://localhost:7248/")
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            builder.Services.AddScoped<UsuarioService>();
            builder.Services.AddScoped<AtencionService>();
            builder.Services.AddSingleton<NotificationService>();

            await builder.Build().RunAsync();
        }
    }
}