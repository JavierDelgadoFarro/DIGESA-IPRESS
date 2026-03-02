using System.Net.Http.Json;
using VisitasTickets.Domain.Dtos;

namespace VisitasTickets.Client.Services
{
    public class AtencionService
    {
        private readonly HttpClient _http;

        public AtencionService(HttpClient http)
        {
            _http = http;
        }

        // Obtener todas las atenciones
        public async Task<List<AtencionDto>> GetAtencionesAsync()
        {
            return await _http.GetFromJsonAsync<List<AtencionDto>>("api/atenciones") ?? new List<AtencionDto>();
        }

        // Obtener atenciones activas (Pendiente y En Ventanilla)
        public async Task<List<AtencionDto>> GetAtencionesActivasAsync()
        {
            return await _http.GetFromJsonAsync<List<AtencionDto>>("api/atenciones/activas") ?? new List<AtencionDto>();
        }

        // Obtener historial de atenciones (Atendidas)
        public async Task<List<AtencionDto>> GetAtencionesHistorialAsync()
        {
            return await _http.GetFromJsonAsync<List<AtencionDto>>("api/atenciones/historial") ?? new List<AtencionDto>();
        }

        // Obtener una atención por ID
        public async Task<AtencionDto?> GetAtencionAsync(int id)
        {
            return await _http.GetFromJsonAsync<AtencionDto>($"api/atenciones/{id}");
        }

        // Crear nueva atención
        public async Task<AtencionDto?> CreateAtencionAsync(AtencionCreateDto atencion)
        {
            var response = await _http.PostAsJsonAsync("api/atenciones", atencion);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AtencionDto>();
            }
            return null;
        }

        // Actualizar estado de atención
        public async Task<bool> UpdateEstadoAtencionAsync(int id, AtencionUpdateDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/atenciones/{id}/estado", dto);
            return response.IsSuccessStatusCode;
        }

        // Obtener tipos de trámite
        public async Task<List<TipoTramiteDto>> GetTiposTramiteAsync()
        {
            return await _http.GetFromJsonAsync<List<TipoTramiteDto>>("api/atenciones/tipos-tramite") ?? new List<TipoTramiteDto>();
        }

        // Obtener tipos preferencial
        public async Task<List<TipoPreferencialDto>> GetTiposPreferencialAsync()
        {
            return await _http.GetFromJsonAsync<List<TipoPreferencialDto>>("api/atenciones/tipos-preferencial") ?? new List<TipoPreferencialDto>();
        }

        // Obtener estados
        public async Task<List<EstadoAtencionDto>> GetEstadosAsync()
        {
            return await _http.GetFromJsonAsync<List<EstadoAtencionDto>>("api/atenciones/estados") ?? new List<EstadoAtencionDto>();
        }

        // Obtener tipos de trabajo
        public async Task<List<TipoTrabajoDto>> GetTiposTrabajoAsync()
        {
            return await _http.GetFromJsonAsync<List<TipoTrabajoDto>>("api/atenciones/tipos-trabajo") ?? new List<TipoTrabajoDto>();
        }

        // Obtener detalles de actividad
        public async Task<List<DetalleActividadDto>> GetDetallesActividadAsync()
        {
            return await _http.GetFromJsonAsync<List<DetalleActividadDto>>("api/atenciones/detalles-actividad") ?? new List<DetalleActividadDto>();
        }

        // Obtener historial de una atención
        public async Task<List<HistorialAtencionDto>> GetHistorialAtencionAsync(int idAtencion)
        {
            return await _http.GetFromJsonAsync<List<HistorialAtencionDto>>($"api/atenciones/{idAtencion}/historial") ?? new List<HistorialAtencionDto>();
        }

        // Obtener tiempo activo en ventanilla (excluyendo pausas)
        public async Task<TiempoActivoDto?> GetTiempoActivoVentanillaAsync(int idAtencion)
        {
            return await _http.GetFromJsonAsync<TiempoActivoDto>($"api/atenciones/{idAtencion}/tiempo-activo");
        }

        // Obtener estadísticas para dashboard del administrador
        public async Task<EstadisticasDashboardDto?> GetEstadisticasDashboardAsync()
        {
            return await _http.GetFromJsonAsync<EstadisticasDashboardDto>("api/dashboard/estadisticas");
        }

        // Obtener estadísticas para dashboard del usuario
        public async Task<EstadisticasUsuarioDto?> GetEstadisticasUsuarioAsync()
        {
            return await _http.GetFromJsonAsync<EstadisticasUsuarioDto>("api/dashboardusuario/estadisticas");
        }

        // Obtener mis atenciones completadas hoy
        public async Task<List<AtencionDto>> GetMisAtencionesHoyAsync()
        {
            return await _http.GetFromJsonAsync<List<AtencionDto>>("api/dashboardusuario/mis-atenciones/hoy") ?? new List<AtencionDto>();
        }

        // Descargar reporte Excel del historial
        public async Task<byte[]?> DescargarHistorialExcelAsync(int? usuarioId = null)
        {
            try
            {
                var url = usuarioId.HasValue 
                    ? $"api/atenciones/historial/excel?usuarioId={usuarioId.Value}"
                    : "api/atenciones/historial/excel";
                
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsByteArrayAsync();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Obtener URL del endpoint de descarga Excel
        public string GetUrlDescargarExcel(int? usuarioId = null)
        {
            var baseUrl = _http.BaseAddress?.ToString().TrimEnd('/') ?? "";
            return usuarioId.HasValue 
                ? $"{baseUrl}/api/atenciones/historial/excel?usuarioId={usuarioId.Value}"
                : $"{baseUrl}/api/atenciones/historial/excel";
        }
    }
}