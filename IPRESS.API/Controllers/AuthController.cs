using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;
using IPRESS.Application.Interfaces;
using IPRESS.Domain.Dtos;
using IPRESS.Domain.Interfaces;

namespace IPRESS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IpressDbContext _context;
        private readonly IAuthService _authService;
        private readonly IAuditUserIdProvider _auditProvider;

        public AuthController(IpressDbContext context, IAuthService authService, IAuditUserIdProvider auditProvider)
        {
            _context = context;
            _authService = authService;
            _auditProvider = auditProvider;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == request.Username);

            if (usuario == null)
                return Unauthorized(new LoginResponse { Message = "Usuario no encontrado." });

            if (!usuario.Activo)
                return Unauthorized(new LoginResponse { Message = "Usuario desactivado." });

            bool passwordValida = VerificarContrasena(request.Password, usuario.Contrasena);
            if (!passwordValida)
                return Unauthorized(new LoginResponse { Message = "Contraseña incorrecta." });

            bool requiereCambio = usuario.NombreUsuario == request.Password;
            if (EsContrasenaEnTextoPlano(usuario.Contrasena))
            {
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(request.Password);
                await _context.SaveChangesAsync();
            }

            var token = _authService.GenerateToken(usuario.IdUsuario, usuario.NombreUsuario, usuario.NombreCompleto);
            if (requiereCambio)
            {
                return Ok(new LoginResponse
                {
                    Token = token,
                    RequiereCambio = true,
                    UsuarioId = usuario.IdUsuario,
                    Message = "Debe cambiar su contraseña antes de continuar."
                });
            }
            return Ok(new LoginResponse
            {
                Token = token,
                RequiereCambio = false,
                UsuarioId = usuario.IdUsuario,
                Message = "Login exitoso."
            });
        }

        private static bool EsContrasenaEnTextoPlano(string contrasenaAlmacenada)
        {
            return !string.IsNullOrEmpty(contrasenaAlmacenada) && !contrasenaAlmacenada.StartsWith("$2", StringComparison.Ordinal);
        }

        private static bool VerificarContrasena(string passwordClaro, string contrasenaAlmacenada)
        {
            if (string.IsNullOrEmpty(contrasenaAlmacenada)) return false;
            if (contrasenaAlmacenada.StartsWith("$2", StringComparison.Ordinal))
                return BCrypt.Net.BCrypt.Verify(passwordClaro, contrasenaAlmacenada);
            return contrasenaAlmacenada == passwordClaro;
        }

        /// <summary>Cambio de contraseña del usuario autenticado (vía SP_Usuario_ActualizarPassword).</summary>
        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequest request)
        {
            var usuarioIdStr = User.FindFirst("UsuarioId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioIdStr) || !int.TryParse(usuarioIdStr, out var id))
                return Unauthorized();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            if (string.IsNullOrWhiteSpace(request.NuevaPassword))
                return BadRequest(new { message = "La nueva contraseña no puede estar vacía." });

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NuevaPassword);
            // Establecer CONTEXT_INFO para que el trigger de auditoría registre UsuarioId e IpOrigen
            var userId = _auditProvider.GetUserId();
            var clientIp = _auditProvider.GetClientIp();
            var bytes = new byte[128];
            if (userId.HasValue)
            {
                var idBytes = BitConverter.GetBytes(userId.Value);
                Array.Copy(idBytes, 0, bytes, 0, Math.Min(4, idBytes.Length));
            }
            if (!string.IsNullOrEmpty(clientIp))
            {
                var ipBytes = System.Text.Encoding.ASCII.GetBytes(clientIp);
                Array.Copy(ipBytes, 0, bytes, 4, Math.Min(45, ipBytes.Length));
            }
            var hex = "0x" + BitConverter.ToString(bytes).Replace("-", "", StringComparison.Ordinal);
            await _context.Database.ExecuteSqlRawAsync($"SET CONTEXT_INFO {hex}");
            await _context.Database.ExecuteSqlRawAsync(
                "EXEC SP_Usuario_ActualizarPassword @p0, @p1", id, passwordHash);

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

        [Authorize]
        [HttpGet("accesos")]
        public async Task<IActionResult> GetAccesos()
        {
            var usuarioId = User.FindFirst("UsuarioId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(usuarioId)) return Unauthorized();

            try
            {
                var accesos = await _authService.GetAccesosAsync(int.Parse(usuarioId));
                return Ok(accesos);
            }
            catch (Exception ex)
            {
#if DEBUG
                return StatusCode(500, new { error = ex.Message, detail = ex.InnerException?.Message });
#else
                return StatusCode(500, new { error = "Error al cargar accesos." });
#endif
            }
        }

    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequest
    {
        public string NuevaPassword { get; set; } = string.Empty;
    }
}
