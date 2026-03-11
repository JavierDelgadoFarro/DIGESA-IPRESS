using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using IPRESS.Client.Services;
using IPRESS.Client.Globals;

namespace IPRESS.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddScoped<Handlers.AuthHandler>();
            builder.Services.AddScoped(sp =>
            {
                var handler = sp.GetRequiredService<Handlers.AuthHandler>();
                handler.InnerHandler = new HttpClientHandler();
                // Si la app se abrió desde otro puerto (ej. Client en 54958), las API deben ir a 5116.
                var baseUrl = builder.HostEnvironment.BaseAddress;
                if (!baseUrl.Contains("5116", StringComparison.OrdinalIgnoreCase))
                    baseUrl = AppConfig.ApiBaseUrl.TrimEnd('/') + "/";
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(baseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

            builder.Services.AddBlazoredLocalStorage();
            builder.Services.AddScoped<AuthService>();
            builder.Services.AddScoped<PermisosService>();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

            builder.Services.AddScoped<UsuarioService>();
            builder.Services.AddScoped<MaestrosService>();
            builder.Services.AddScoped<GestionUsuariosService>();
            builder.Services.AddRadzenComponents();
            builder.Services.AddScoped<IPRESS.Client.Services.NotificationService>(sp => new IPRESS.Client.Services.NotificationService(sp.GetRequiredService<Radzen.NotificationService>()));

            await builder.Build().RunAsync();
        }
    }
}