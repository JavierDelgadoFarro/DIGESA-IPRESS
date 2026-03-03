using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitasTickets.Infrastructure.Persistence;
using VisitasTickets.Application.Interfaces;
using VisitasTickets.Domain.Dtos;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAuthService _authService;

        public AuthController(AppDbContext context, IAuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var usuario = await _context.AdmUsuarios
                .FirstOrDefaultAsync(u => u.NombreUsuarioUsu == request.Username);

            if (usuario == null)
                return Unauthorized(new LoginResponse { Message = "Usuario no encontrado." });

            bool valido = request.Password == usuario.ContrasenaUsu;
            if (!valido)
                return Unauthorized(new LoginResponse { Message = "Contraseña incorrecta." });

            if (usuario.NombreUsuarioUsu == usuario.ContrasenaUsu)
            {
                return Ok(new LoginResponse
                {
                    RequiereCambio = true,
                    UsuarioId = usuario.IdUsuario,
                    Message = "Debe cambiar su contraseña antes de continuar."
                });
            }

            var token = await _authService.AuthenticateAsync(request.Username, request.Password);
            if (token == null)
                return Unauthorized(new LoginResponse { Message = "Credenciales inválidas o área no autorizada." });

            return Ok(new LoginResponse
            {
                Token = token,
                RequiereCambio = false,
                UsuarioId = usuario.IdUsuario,
                Message = "Login exitoso."
            });
        }

        [HttpPost("{id}/cambiar-password")]
        public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordRequest request)
        {
            var usuario = await _context.AdmUsuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado." });

            if (string.IsNullOrWhiteSpace(request.NuevaPassword))
                return BadRequest(new { message = "La nueva contraseña no puede estar vacía." });

            usuario.ContrasenaUsu = request.NuevaPassword;
            _context.Entry(usuario).Property(u => u.ContrasenaUsu).IsModified = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Contraseña actualizada correctamente." });
        }

        [Authorize]
        [HttpGet("accesos")]
        public async Task<IActionResult> GetAccesos()
        {
            var usuarioId = User.FindFirst("UsuarioId")?.Value;
            if (usuarioId == null) return Unauthorized();

            var accesos = await _authService.GetAccesosAsync(int.Parse(usuarioId));
            return Ok(accesos);
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequest
    {
        public string NuevaPassword { get; set; }
    }
}
