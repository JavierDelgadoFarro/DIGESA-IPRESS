using Microsoft.AspNetCore.SignalR;

namespace VisitasTickets.API.Hubs
{
    public class SignalRNotificationService
    {
        private readonly IHubContext<AtencionHub> _hubContext;

        public SignalRNotificationService(IHubContext<AtencionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotificarNuevaAtencion()
        {
            Console.WriteLine("SignalR Backend: Enviando notificación NuevaAtencion a todos los clientes");
            await _hubContext.Clients.All.SendAsync("NuevaAtencion");
        }

        public async Task NotificarCambioEstadoAtencion(int idAtencion)
        {
            Console.WriteLine($"SignalR Backend: Enviando notificación CambioEstadoAtencion (ID: {idAtencion}) a todos los clientes");
            await _hubContext.Clients.All.SendAsync("CambioEstadoAtencion", idAtencion);
        }

        public async Task NotificarActualizacionDashboard()
        {
            Console.WriteLine("SignalR Backend: Enviando notificación ActualizacionDashboard a todos los clientes");
            await _hubContext.Clients.All.SendAsync("ActualizacionDashboard");
        }

        public async Task NotificarAUsuario(int usuarioId, string mensaje, object? data = null)
        {
            await _hubContext.Clients.Group($"Usuario_{usuarioId}")
                .SendAsync("NotificacionPersonal", mensaje, data);
        }
    }
}
