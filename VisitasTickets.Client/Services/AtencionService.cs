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
    }
}
