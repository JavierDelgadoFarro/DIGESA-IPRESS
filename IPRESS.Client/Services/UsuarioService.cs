using System.Net.Http.Json;

namespace IPRESS.Client.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _http;

        public UsuarioService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> CambiarPasswordAsync(string nuevaPassword)
        {
            var response = await _http.PostAsJsonAsync("api/auth/cambiar-password", new { NuevaPassword = nuevaPassword });
            return response.IsSuccessStatusCode;
        }
    }
}
