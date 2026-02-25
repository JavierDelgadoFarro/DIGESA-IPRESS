using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace VisitasTickets.API.Hubs
{
    [Authorize]
    public class AtencionHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var usuarioId = Context.User?.FindFirst("UsuarioId")?.Value;
            if (!string.IsNullOrEmpty(usuarioId))
            {
                // Agregar el usuario a un grupo con su ID para notificaciones específicas
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Usuario_{usuarioId}");
                Console.WriteLine($"Usuario {usuarioId} conectado a SignalR. ConnectionId: {Context.ConnectionId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var usuarioId = Context.User?.FindFirst("UsuarioId")?.Value;
            if (!string.IsNullOrEmpty(usuarioId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Usuario_{usuarioId}");
                Console.WriteLine($"Usuario {usuarioId} desconectado de SignalR");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Métodos para notificar cambios en atenciones
        public async Task NotificarNuevaAtencion()
        {
            await Clients.All.SendAsync("NuevaAtencion");
        }

        public async Task NotificarCambioEstadoAtencion(int idAtencion)
        {
            await Clients.All.SendAsync("CambioEstadoAtencion", idAtencion);
        }

        public async Task NotificarActualizacionDashboard()
        {
            await Clients.All.SendAsync("ActualizacionDashboard");
        }
    }
}
