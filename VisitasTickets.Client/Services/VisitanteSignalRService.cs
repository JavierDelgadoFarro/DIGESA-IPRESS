using Blazored.LocalStorage;
using Microsoft.AspNetCore.SignalR.Client;
using VisitasTickets.Client.Globals;

namespace VisitasTickets.Client.Services
{
    /// <summary>
    /// Servicio SignalR para visitantes (sin autenticación)
    /// </summary>
    public class VisitanteSignalRService : IAsyncDisposable
    {
        private HubConnection? _hubConnection;
        private readonly string _hubUrl;

        public event Action<string, string, int>? OnTurnoActualizado;
        public event Action? OnActualizacionTurnos;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public VisitanteSignalRService()
        {
            _hubUrl = $"{AppConfig.ApiBaseUrl}/hubs/visitantes";
        }

        public async Task IniciarConexionAsync(string documento)
        {
            if (_hubConnection != null && _hubConnection.State == HubConnectionState.Connected)
            {
                return;
            }

            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl)
                    .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();

                // Configurar los handlers de eventos
                _hubConnection.On<object>("TurnoActualizado", (data) =>
                {
                    try
                    {
                        var json = System.Text.Json.JsonSerializer.Serialize(data);
                        var turnoData = System.Text.Json.JsonDocument.Parse(json);
                        
                        var doc = turnoData.RootElement.GetProperty("Documento").GetString() ?? "";
                        var mensaje = turnoData.RootElement.GetProperty("Mensaje").GetString() ?? "";
                        var numero = turnoData.RootElement.GetProperty("NumeroAtencion").GetInt32();
                        
                        OnTurnoActualizado?.Invoke(doc, mensaje, numero);
                    }
                    catch (Exception ex)
                    {
                        // Error al parsear evento
                    }
                });

                _hubConnection.On("ActualizacionTurnos", () =>
                {
                    OnActualizacionTurnos?.Invoke();
                });

                _hubConnection.Reconnecting += error =>
                {
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += async connectionId =>
                {
                    await UnirseAGrupoDocumento(documento);
                    return;
                };

                _hubConnection.Closed += error =>
                {
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                await UnirseAGrupoDocumento(documento);
            }
            catch (Exception ex)
            {
                // Error al conectar
            }
        }

        public async Task UnirseAGrupoDocumento(string documento)
        {
            if (_hubConnection?.State == HubConnectionState.Connected)
            {
                try
                {
                    await _hubConnection.InvokeAsync("UnirseAGrupoDocumento", documento);
                }
                catch (Exception ex)
                {
                    // Error al unirse al grupo
                }
            }
        }

        public async Task DetenerConexionAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    await _hubConnection.StopAsync();
                }
                catch (Exception ex)
                {
                    // Error al detener conexión
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}
