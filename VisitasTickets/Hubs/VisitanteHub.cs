using Microsoft.AspNetCore.SignalR;

namespace VisitasTickets.API.Hubs
{
    /// <summary>
    /// Hub público para visitantes (sin autenticación)
    /// Permite recibir notificaciones en tiempo real sobre el estado de su turno
    /// </summary>
    public class VisitanteHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Permite al visitante unirse a un grupo basado en su documento
        /// </summary>
        public async Task UnirseAGrupoDocumento(string documento)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Documento_{documento}");
        }

        /// <summary>
        /// Permite al visitante salir del grupo de su documento
        /// </summary>
        public async Task SalirDeGrupoDocumento(string documento)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Documento_{documento}");
            Console.WriteLine($"Visitante con documento {documento} salió del grupo.");
        }
    }
}
