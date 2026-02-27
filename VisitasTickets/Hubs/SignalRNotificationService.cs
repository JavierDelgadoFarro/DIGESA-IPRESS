using Microsoft.AspNetCore.SignalR;

namespace VisitasTickets.API.Hubs
{
    public class SignalRNotificationService
    {
        private readonly IHubContext<AtencionHub> _hubContext;
        private readonly IHubContext<VisitanteHub> _visitanteHubContext;

        public SignalRNotificationService(
            IHubContext<AtencionHub> hubContext,
            IHubContext<VisitanteHub> visitanteHubContext)
        {
            _hubContext = hubContext;
            _visitanteHubContext = visitanteHubContext;
        }

        public async Task NotificarNuevaAtencion()
        {
            await _hubContext.Clients.All.SendAsync("NuevaAtencion");
        }

        public async Task NotificarCambioEstadoAtencion(int idAtencion)
        {
            await _hubContext.Clients.All.SendAsync("CambioEstadoAtencion", idAtencion);
        }

        public async Task NotificarActualizacionDashboard()
        {
            await _hubContext.Clients.All.SendAsync("ActualizacionDashboard");
        }

        public async Task NotificarAUsuario(int usuarioId, string mensaje, object? data = null)
        {
            await _hubContext.Clients.Group($"Usuario_{usuarioId}")
                .SendAsync("NotificacionPersonal", mensaje, data);
        }

        /// <summary>
        /// Notifica a un visitante específico (por documento) que es su turno
        /// </summary>
        public async Task NotificarTurnoVisitante(string documento, string mensaje, int numeroAtencion)
        {
            await _visitanteHubContext.Clients.Group($"Documento_{documento}")
                .SendAsync("TurnoActualizado", new
                {
                    Documento = documento,
                    Mensaje = mensaje,
                    NumeroAtencion = numeroAtencion,
                    EsSuTurno = true,
                    Timestamp = DateTime.Now
                });
        }

        /// <summary>
        /// Notifica a todos los visitantes conectados sobre actualizaciones generales
        /// </summary>
        public async Task NotificarActualizacionTurnos()
        {
            await _visitanteHubContext.Clients.All.SendAsync("ActualizacionTurnos");
        }
    }
}
