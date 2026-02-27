using System.Net.Http.Json;
using VisitasTickets.Domain.Dtos;

namespace VisitasTickets.Client.Services
{
    public class VisitanteService
    {
        private readonly HttpClient _httpClient;

        public VisitanteService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<RegistroPublicoResponseDto?> RegistrarAtencionAsync(AtencionCreateDto atencion)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/publico/registro", atencion);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<RegistroPublicoResponseDto>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error al registrar atención: {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar atención: {ex.Message}");
                return null;
            }
        }

        public async Task<TurnoConsultaDto?> ConsultarTurnoPorIdAsync(int idAtencion)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/publico/turno/id/{idAtencion}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<TurnoConsultaDto>();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al consultar turno: {ex.Message}");
                return null;
            }
        }

        public async Task<List<TipoTramiteDto>> GetTramitesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/publico/tramites");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<TipoTramiteDto>>() ?? new List<TipoTramiteDto>();
                }

                return new List<TipoTramiteDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener trámites: {ex.Message}");
                return new List<TipoTramiteDto>();
            }
        }

        public async Task<List<TipoPreferencialDto>> GetTiposPreferencialAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/publico/tipos-preferencial");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<TipoPreferencialDto>>() ?? new List<TipoPreferencialDto>();
                }

                return new List<TipoPreferencialDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener tipos preferenciales: {ex.Message}");
                return new List<TipoPreferencialDto>();
            }
        }

        public async Task<DatosPersonaDto?> BuscarPorDocumentoAsync(string documento)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/publico/buscar-por-documento/{documento}");
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<DatosPersonaDto>();
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al buscar documento: {ex.Message}");
                return null;
            }
        }
    }
}
