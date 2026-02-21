using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Entities;
using VisitasTickets.Domain.Globals;
using VisitasTickets.Infrastructure.Persistence;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            var usuarios = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .Where(u => u.IdArea == AppConfig.DefaultAreaId)
                .Select(u => new UsuarioDto
                {
                    IdUsuario = u.IdUsuario,
                    NombreUsuarioUsu = u.NombreUsuarioUsu,
                    ApellidosNombrePer = u.IdPersonalNavigation.ApellidosNombrePer,
                    EstadoTexto = u.IdEstado == 1 ? "Activo" : "Inactivo"
                })
                .ToListAsync();

            return usuarios;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (usuario == null) return NotFound();

            return new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuarioUsu = usuario.NombreUsuarioUsu,
                ApellidosNombrePer = usuario.IdPersonalNavigation?.ApellidosNombrePer
            };
        }

        [HttpGet("{id}/completo")]
        public async Task<ActionResult<UsuarioEdicionDto>> GetUsuarioCompleto(int id)
        {
            var usuario = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (usuario == null) return NotFound();

            return new UsuarioEdicionDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuarioUsu = usuario.NombreUsuarioUsu,
                Paterno = usuario.IdPersonalNavigation?.Paterno,
                Materno = usuario.IdPersonalNavigation?.Materno,
                Nombre = usuario.IdPersonalNavigation?.Nombre
            };
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(AdmUsuario usuario)
        {
            usuario.IdArea = AppConfig.DefaultAreaId;
            usuario.IdSede = 1;
            usuario.IdEstado = 1;
            usuario.ContrasenaUsu = usuario.NombreUsuarioUsu;

            if (usuario.IdPersonalNavigation != null)
            {
                var personal = usuario.IdPersonalNavigation;

                var maxId = await _context.AdmPersonals.MaxAsync(p => (int?)p.IdPersonal) ?? 0;
                personal.IdPersonal = maxId + 1;

                personal.ApellidosNombrePer = $"{personal.Nombre} {personal.Paterno} {personal.Materno}".Trim();
                personal.IdArea = AppConfig.DefaultAreaId;
                personal.IdProfesion = AppConfig.DefaultProfesionId;
                personal.IdCargo = AppConfig.DefaultCargoId;
                personal.IdEstado = 1;

                _context.AdmPersonals.Add(personal);
                await _context.SaveChangesAsync();

                usuario.IdPersonal = personal.IdPersonal;
            }

            _context.AdmUsuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var dto = new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuarioUsu = usuario.NombreUsuarioUsu,
                ApellidosNombrePer = usuario.IdPersonalNavigation?.ApellidosNombrePer
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, dto);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioDto>> UpdateUsuario(int id, AdmUsuario usuario)
        {
            var existente = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (existente == null) return NotFound();

            existente.NombreUsuarioUsu = usuario.NombreUsuarioUsu;

            if (existente.IdPersonalNavigation != null && usuario.IdPersonalNavigation != null)
            {
                var personal = existente.IdPersonalNavigation;
                personal.Paterno = usuario.IdPersonalNavigation.Paterno;
                personal.Materno = usuario.IdPersonalNavigation.Materno;
                personal.Nombre = usuario.IdPersonalNavigation.Nombre;
                personal.ApellidosNombrePer = $"{personal.Paterno} {personal.Materno} {personal.Nombre}".Trim();
            }

            await _context.SaveChangesAsync();

            var dto = new UsuarioDto
            {
                IdUsuario = existente.IdUsuario,
                NombreUsuarioUsu = existente.NombreUsuarioUsu,
                ApellidosNombrePer = existente.IdPersonalNavigation?.ApellidosNombrePer
            };

            return dto;
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (usuario == null) return NotFound();

            usuario.IdEstado = usuario.IdEstado == 1 ? 0 : 1;
            _context.Entry(usuario).State = EntityState.Modified;

            if (usuario.IdPersonalNavigation != null)
            {
                usuario.IdPersonalNavigation.IdEstado = usuario.IdPersonalNavigation.IdEstado == 1 ? 0 : 1;
                _context.Entry(usuario.IdPersonalNavigation).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"Usuario {(usuario.IdEstado == 1 ? "activado" : "desactivado")} correctamente." });
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var usuario = await _context.AdmUsuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.ContrasenaUsu = usuario.NombreUsuarioUsu;
            _context.Entry(usuario).Property(u => u.ContrasenaUsu).IsModified = true;

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
