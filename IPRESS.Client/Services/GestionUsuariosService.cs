using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IPRESS.Client.Services
{
    public class GestionUsuariosService
    {
        private readonly HttpClient _http;

        /// <summary>Opciones JSON para que la API (camelCase) se deserialice correctamente en los DTOs.</summary>
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public GestionUsuariosService(HttpClient http) => _http = http;

        public async Task<List<UsuarioListDto>> GetUsuariosAsync()
        {
            var list = await _http.GetFromJsonAsync<List<UsuarioListDto>>("api/gestionusuarios/usuarios", JsonOptions);
            return list ?? new List<UsuarioListDto>();
        }

        public async Task<UsuarioEditDto?> GetUsuarioAsync(int id)
        {
            return await _http.GetFromJsonAsync<UsuarioEditDto>($"api/gestionusuarios/usuarios/{id}", JsonOptions);
        }

        public async Task<bool> PostUsuarioAsync(UsuarioCreateDto dto)
        {
            var r = await _http.PostAsJsonAsync("api/gestionusuarios/usuarios", dto);
            return r.IsSuccessStatusCode;
        }

        public async Task<bool> PutUsuarioAsync(int id, UsuarioUpdateDto dto)
        {
            var r = await _http.PutAsJsonAsync($"api/gestionusuarios/usuarios/{id}", dto);
            return r.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteUsuarioAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/gestionusuarios/usuarios/{id}");
            if (res.IsSuccessStatusCode) return (true, null);
            var body = await res.Content.ReadAsStringAsync();
            string? msg = null;
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var m)) msg = m.GetString();
            }
            catch { msg = body; }
            return (false, msg ?? "No se pudo eliminar el usuario.");
        }

        public async Task<List<RolDto>> GetRolesAsync()
        {
            var list = await _http.GetFromJsonAsync<List<RolDto>>("api/gestionusuarios/roles", JsonOptions);
            return list ?? new List<RolDto>();
        }

        public async Task<EstructuraPermisosDto?> GetEstructuraAsync()
        {
            return await _http.GetFromJsonAsync<EstructuraPermisosDto>("api/gestionusuarios/estructura", JsonOptions);
        }

        public async Task<PermisosRolDto?> GetPermisosRolAsync(int idRol)
        {
            return await _http.GetFromJsonAsync<PermisosRolDto>($"api/gestionusuarios/roles/{idRol}/permisos", JsonOptions);
        }

        public async Task<bool> PutPermisosRolAsync(int idRol, PermisosRolDto dto)
        {
            var r = await _http.PutAsJsonAsync($"api/gestionusuarios/roles/{idRol}/permisos", dto);
            return r.IsSuccessStatusCode;
        }

        public async Task<RolDto?> PostRolAsync(string codigo, string nombre)
        {
            var r = await _http.PostAsJsonAsync("api/gestionusuarios/roles", new { Codigo = codigo, Nombre = nombre });
            if (!r.IsSuccessStatusCode) return null;
            return await r.Content.ReadFromJsonAsync<RolDto>(JsonOptions);
        }

        public async Task<bool> PutRolAsync(int id, string codigo, string nombre)
        {
            var r = await _http.PutAsJsonAsync($"api/gestionusuarios/roles/{id}", new { Codigo = codigo, Nombre = nombre });
            return r.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string? ErrorMessage)> DeleteRolAsync(int id)
        {
            var res = await _http.DeleteAsync($"api/gestionusuarios/roles/{id}");
            if (res.IsSuccessStatusCode) return (true, null);
            var body = await res.Content.ReadAsStringAsync();
            string? msg = null;
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var m)) msg = m.GetString();
            }
            catch { msg = body; }
            return (false, msg ?? "No se pudo eliminar el rol.");
        }

        /// <summary>Permisos fusionados (sin duplicados) para una lista de roles.</summary>
        public async Task<PermisosRolDto?> GetPermisosPorRolesAsync(IEnumerable<int> rolIds)
        {
            var list = rolIds?.Distinct().ToList() ?? new List<int>();
            if (list.Count == 0)
                return new PermisosRolDto();
            var r = await _http.PostAsJsonAsync("api/gestionusuarios/permisos-por-roles", new { RolIds = list });
            return r.IsSuccessStatusCode ? await r.Content.ReadFromJsonAsync<PermisosRolDto>(JsonOptions) : null;
        }
    }

    public class UsuarioListDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public int? IdEstablecimiento { get; set; }
        [JsonPropertyName("diresaNombre")]
        public string? DiresaNombre { get; set; }
        [JsonPropertyName("redNombre")]
        public string? RedNombre { get; set; }
        [JsonPropertyName("microRedNombre")]
        public string? MicroRedNombre { get; set; }
        [JsonPropertyName("establecimientoNombre")]
        public string? EstablecimientoNombre { get; set; }
        [JsonPropertyName("roles")]
        public List<int> Roles { get; set; } = new();
    }

    public class UsuarioEditDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string? Email { get; set; }
        public bool Activo { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public int? IdEstablecimiento { get; set; }
        [JsonPropertyName("rolIds")]
        public List<int> RolIds { get; set; } = new();
    }

    public class UsuarioCreateDto
    {
        public string NombreUsuario { get; set; } = "";
        public string? Contrasena { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public int? IdEstablecimiento { get; set; }
        public List<int>? RolIds { get; set; }
    }

    public class UsuarioUpdateDto
    {
        public string NombreCompleto { get; set; } = "";
        public string? Email { get; set; }
        public string? Contrasena { get; set; }
        public bool Activo { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public int? IdEstablecimiento { get; set; }
        public List<int>? RolIds { get; set; }
    }

    public class RolDto
    {
        public int IdRol { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class EstructuraPermisosDto
    {
        [JsonPropertyName("estructura")]
        public List<ModuloEstructuraDto> Estructura { get; set; } = new();
        [JsonPropertyName("botonesPorModulo")]
        public List<BotonDto> BotonesPorModulo { get; set; } = new();
    }

    public class ModuloEstructuraDto
    {
        [JsonPropertyName("idModulo")]
        public int IdModulo { get; set; }
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = "";
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = "";
        [JsonPropertyName("menus")]
        public List<MenuEstructuraDto> Menus { get; set; } = new();
    }

    public class MenuEstructuraDto
    {
        [JsonPropertyName("idMenu")]
        public int IdMenu { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = "";
        [JsonPropertyName("subMenus")]
        public List<SubMenuDto> SubMenus { get; set; } = new();
    }

    public class SubMenuDto
    {
        [JsonPropertyName("idSubMenu")]
        public int IdSubMenu { get; set; }
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = "";
        [JsonPropertyName("ruta")]
        public string Ruta { get; set; } = "";
    }

    public class BotonDto
    {
        [JsonPropertyName("idBoton")]
        public int IdBoton { get; set; }
        [JsonPropertyName("idModulo")]
        public int IdModulo { get; set; }
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = "";
        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = "";
    }

    public class PermisosRolDto
    {
        [JsonPropertyName("subMenuIds")]
        public List<int> SubMenuIds { get; set; } = new();
        [JsonPropertyName("botonIds")]
        public List<int> BotonIds { get; set; } = new();
    }
}
