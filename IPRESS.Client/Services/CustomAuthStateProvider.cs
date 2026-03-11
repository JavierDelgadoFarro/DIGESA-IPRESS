using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Text.Json;

namespace IPRESS.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;
        private ClaimsPrincipal? _cachedUser;

        public CustomAuthStateProvider(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                var token = await _localStorage.GetItemAsync<string>("authToken");

                if (string.IsNullOrWhiteSpace(token))
                {
                    return _anonymous;
                }

                // Validar que el token tenga formato JWT válido
                var claims = ParseClaimsFromJwt(token);
                if (claims == null || !claims.Any())
                {
                    await _localStorage.RemoveItemAsync("authToken");
                    return _anonymous;
                }

                // Verificar expiración del token
                var expClaim = claims.FirstOrDefault(c => c.Type == "exp");
                if (expClaim != null)
                {
                    if (long.TryParse(expClaim.Value, out var exp))
                    {
                        var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;

                        if (expirationDate < DateTime.UtcNow)
                        {
                            await _localStorage.RemoveItemAsync("authToken");
                            _cachedUser = null;
                            return _anonymous;
                        }
                    }
                }

                // Token válido - crear identidad autenticada y cachear
                var identity = new ClaimsIdentity(claims, "jwt");
                _cachedUser = new ClaimsPrincipal(identity);

                return new AuthenticationState(_cachedUser);
            }
            catch (Exception)
            {
                // No invalidar sesión por errores transitorios (ej. localStorage, red).
                // Si ya teníamos usuario en caché, mantenerlo.
                if (_cachedUser != null)
                    return new AuthenticationState(_cachedUser);
                return _anonymous;
            }
        }

        public void NotifyUserAuthentication(string token)
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            _cachedUser = new ClaimsPrincipal(identity);

            var authState = Task.FromResult(new AuthenticationState(_cachedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void NotifyUserLogout()
        {
            _cachedUser = null;
            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            try
            {
                var claims = new List<Claim>();
                var parts = jwt.Split('.');

                if (parts.Length != 3)
                    return claims;

                var payload = parts[1];

                // Ajustar padding para Base64
                var base64 = payload.Replace('-', '+').Replace('_', '/');
                switch (payload.Length % 4)
                {
                    case 2: base64 += "=="; break;
                    case 3: base64 += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(base64);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs != null)
                {
                    foreach (var kvp in keyValuePairs)
                    {
                        if (kvp.Value is JsonElement element)
                        {
                            claims.Add(new Claim(kvp.Key, element.ToString()));
                        }
                        else
                        {
                            claims.Add(new Claim(kvp.Key, kvp.Value?.ToString() ?? string.Empty));
                        }
                    }

                    // Mapear claims específicos
                    if (keyValuePairs.ContainsKey("sub"))
                    {
                        var subValue = keyValuePairs["sub"];
                        if (subValue is JsonElement subElement)
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, subElement.ToString()));
                        else
                            claims.Add(new Claim(ClaimTypes.NameIdentifier, subValue?.ToString() ?? ""));
                    }

                    if (keyValuePairs.ContainsKey("name"))
                    {
                        var nameValue = keyValuePairs["name"];
                        if (nameValue is JsonElement nameElement)
                            claims.Add(new Claim(ClaimTypes.Name, nameElement.ToString()));
                        else
                            claims.Add(new Claim(ClaimTypes.Name, nameValue?.ToString() ?? ""));
                    }

                    if (keyValuePairs.ContainsKey("email"))
                    {
                        var emailValue = keyValuePairs["email"];
                        if (emailValue is JsonElement emailElement)
                            claims.Add(new Claim(ClaimTypes.Email, emailElement.ToString()));
                        else
                            claims.Add(new Claim(ClaimTypes.Email, emailValue?.ToString() ?? ""));
                    }

                    if (keyValuePairs.ContainsKey("role"))
                    {
                        var roleValue = keyValuePairs["role"];
                        if (roleValue is JsonElement roleElement)
                        {
                            if (roleElement.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var role in roleElement.EnumerateArray())
                                {
                                    claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? ""));
                                }
                            }
                            else
                            {
                                claims.Add(new Claim(ClaimTypes.Role, roleElement.ToString()));
                            }
                        }
                        else
                        {
                            claims.Add(new Claim(ClaimTypes.Role, roleValue?.ToString() ?? ""));
                        }
                    }
                }

                return claims;
            }
            catch
            {
                return new List<Claim>();
            }
        }
    }
}