using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using VisitasTickets.Client.Globals;

namespace VisitasTickets.Client.Services
{
    public class SignalRService : IAsyncDisposable
    {
        private readonly ILocalStorageService _localStorage;
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public event Action? OnNuevaAtencion;
        public event Action<int>? OnCambioEstadoAtencion;
        public event Action? OnActualizacionDashboard;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public SignalRService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _hubUrl = $"{AppConfig.ApiBaseUrl}/hubs/atenciones";
            Console.WriteLine($"SignalR: URL configurada: {_hubUrl}");
        }

        public async Task IniciarConexionAsync()
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                Console.WriteLine("SignalR: Ya conectado");
                return; // Ya está conectado
            }

            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                Console.WriteLine($"SignalR: Conexión en estado {_hubConnection.State}, esperando...");
                return;
            }

            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");
                
                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("SignalR: No hay token disponible");
                    return;
                }

                Console.WriteLine("SignalR: Construyendo conexión...");

                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token)!;
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();

                // Configurar los handlers de eventos
                _hubConnection.On("NuevaAtencion", () =>
                {
                    Console.WriteLine("SignalR: Nueva atención recibida");
                    OnNuevaAtencion?.Invoke();
                });

                _hubConnection.On<int>("CambioEstadoAtencion", (idAtencion) =>
                {
                    Console.WriteLine($"SignalR: Cambio de estado en atención {idAtencion}");
                    OnCambioEstadoAtencion?.Invoke(idAtencion);
                });

                _hubConnection.On("ActualizacionDashboard", () =>
                {
                    Console.WriteLine("SignalR: Actualización de dashboard");
                    OnActualizacionDashboard?.Invoke();
                });

                _hubConnection.Reconnecting += error =>
                {
                    Console.WriteLine($"SignalR: Reconectando... {error?.Message}");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    Console.WriteLine($"SignalR: Reconectado. ConnectionId: {connectionId}");
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += error =>
                {
                    Console.WriteLine($"SignalR: Conexión cerrada. {error?.Message}");
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                Console.WriteLine($"SignalR: Conectado. Estado: {_hubConnection.State}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al iniciar conexión SignalR: {ex.Message}");
            }
        }

        public async Task DetenerConexionAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                    Console.WriteLine("SignalR: Conexión detenida");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al detener conexión SignalR: {ex.Message}");
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
                _hubConnection = null;
            }
        }
    }
}
