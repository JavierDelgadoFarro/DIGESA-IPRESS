using System.Security.Claims;
using IPRESS.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace IPRESS.API.Services
{
    /// <summary>Obtiene el UsuarioId y la IP del cliente para auditoría en BD (CONTEXT_INFO).</summary>
    public class HttpContextAuditUserIdProvider : IAuditUserIdProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextAuditUserIdProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;
            var idStr = user.FindFirst("UsuarioId")?.Value ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(idStr, out var id) ? id : null;
        }

        public string? GetClientIp()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return null;
            var ip = ctx.Connection?.RemoteIpAddress?.ToString();
            if (!string.IsNullOrEmpty(ip) && ip.Length > 45) ip = ip.Substring(0, 45);
            return ip;
        }
    }
}
