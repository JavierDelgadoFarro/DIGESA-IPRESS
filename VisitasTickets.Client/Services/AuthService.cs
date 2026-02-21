using Blazored.LocalStorage;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using VisitasTickets.Domain.Dtos;

namespace VisitasTickets.Client.Services
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

            return await _httpClient.GetFromJsonAsync<AccesosResponse>("api/Auth/accesos");
        }

    }
}
