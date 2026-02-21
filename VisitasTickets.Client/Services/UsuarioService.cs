using System.Net.Http;
using System.Net.Http.Json;
using VisitasTickets.Client.Globals;
using VisitasTickets.Client.Models;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Entities;

namespace VisitasTickets.Client.Services
{
    public class UsuarioService
    {
        private readonly HttpClient _http;

        public UsuarioService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<UsuarioModel>> GetUsuariosAsync()
        {
            return await _http.GetFromJsonAsync<List<UsuarioModel>>("api/usuarios");
        }

        public async Task<UsuarioModel?> GetUsuarioAsync(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioModel>($"api/usuarios/{id}");
        }

        public async Task<UsuarioEdicionDto?> GetUsuarioCompletoAsync(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioEdicionDto>($"api/usuarios/{id}/completo");
        }

        public async Task<List<MenuDto>> GetMenusDisponiblesAsync()
        {
            return await _http.GetFromJsonAsync<List<MenuDto>>("api/usuarios/menus-disponibles") ?? new List<MenuDto>();
        }

        public async Task<bool> CreateUsuarioAsync(UsuarioEdicionDto usuario)
        {
            var response = await _http.PostAsJsonAsync("api/usuarios", usuario);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUsuarioAsync(int id, UsuarioEdicionDto usuario)
        {
            var response = await _http.PutAsJsonAsync($"api/usuarios/{id}", usuario);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUsuarioAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/usuarios/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(int idUsuario)
        {
            var response = await _http.PostAsync($"api/usuarios/{idUsuario}/reset-password", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CambiarPasswordAsync(int idUsuario, string nuevaPassword)
        {
            var response = await _http.PostAsJsonAsync($"api/auth/{idUsuario}/cambiar-password",
                new { NuevaPassword = nuevaPassword });

            return response.IsSuccessStatusCode;
        }

    }

}
