using System.Text.Json;
using Blazored.LocalStorage;
using System.Net.Http.Json;
using IPRESS.Domain.Dtos;

namespace IPRESS.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public async Task<LoginResponse> LoginAsync(string username, string password)
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { Username = username, Password = password });

            if (!response.IsSuccessStatusCode)
            {
                return new LoginResponse { Message = "Credenciales inválidas." };
            }

            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result == null)
                return new LoginResponse { Message = "Error al procesar la respuesta del servidor." };

            if (!string.IsNullOrEmpty(result.Token))
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
            }

            return result;
        }


        public async Task<string?> GetTokenAsync()
        {
            return await _localStorage.GetItemAsync<string>("authToken");
        }

        public async Task LogoutAsync()
        {
            await _localStorage.RemoveItemAsync("authToken");
        }

        public async Task<AccesosResponse?> GetAccesosAsync()
        {
            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token)) return null;

            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var response = await _httpClient.GetAsync("api/Auth/accesos");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<AccesosResponse>(options);
        }

    }
}
