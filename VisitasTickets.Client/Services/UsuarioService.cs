using System.Net.Http.Json;

namespace VisitasTickets.Client.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _http;

        public UsuarioService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> CambiarPasswordAsync(int idUsuario, string nuevaPassword)
        {
            var response = await _http.PostAsJsonAsync($"api/auth/{idUsuario}/cambiar-password",
                new { NuevaPassword = nuevaPassword });
            return response.IsSuccessStatusCode;
        }
    }
}
