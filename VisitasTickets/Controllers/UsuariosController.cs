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
                .Include(u => u.AdmDetalleSubMenus)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (usuario == null) return NotFound();

            var subMenusIds = usuario.AdmDetalleSubMenus.Select(d => d.IdSmenu ?? 0).ToList();

            return new UsuarioEdicionDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuarioUsu = usuario.NombreUsuarioUsu,
                Paterno = usuario.IdPersonalNavigation?.Paterno,
                Materno = usuario.IdPersonalNavigation?.Materno,
                Nombre = usuario.IdPersonalNavigation?.Nombre,
                SubMenusSeleccionados = subMenusIds
            };
        }

        [HttpGet("menus-disponibles")]
        public async Task<ActionResult<IEnumerable<MenuDto>>> GetMenusDisponibles()
        {
            var menus = await _context.AdmMenus
                .Include(m => m.AdmSubMenus.Where(sm => sm.IdEstado == 1))
                .Where(m => m.IdEstado == 1 && m.IdModulo == AppConfig.DefaultModuloId)
                .OrderBy(m => m.OrdenMen)
                .Select(m => new MenuDto
                {
                    IdMenu = m.IdMenu,
                    NombreMenu = m.DescripcionMen ?? "",
                    SubMenus = m.AdmSubMenus
                        .OrderBy(sm => sm.OrdenSme)
                        .Select(sm => new SubMenuDto
                        {
                            IdSubMenu = sm.IdSmenu,
                            NombreSubMenu = sm.DescripcionSme ?? "",
                            Ruta = sm.RutaWebSme
                        }).ToList()
                })
                .ToListAsync();

            return menus;
        }

        [HttpPost]
        public async Task<ActionResult<UsuarioDto>> CreateUsuario(UsuarioEdicionDto dto)
        {
            var usuario = new AdmUsuario
            {
                NombreUsuarioUsu = dto.NombreUsuarioUsu,
                IdArea = AppConfig.DefaultAreaId,
                IdSede = 1,
                IdEstado = 1,
                ContrasenaUsu = dto.NombreUsuarioUsu
            };

            // Crear personal
            var maxId = await _context.AdmPersonals.MaxAsync(p => (int?)p.IdPersonal) ?? 0;
            var personal = new AdmPersonal
            {
                IdPersonal = maxId + 1,
                Nombre = dto.Nombre,
                Paterno = dto.Paterno,
                Materno = dto.Materno,
                ApellidosNombrePer = $"{dto.Nombre} {dto.Paterno} {dto.Materno} ".Trim(),
                IdArea = AppConfig.DefaultAreaId,
                IdProfesion = AppConfig.DefaultProfesionId,
                IdCargo = AppConfig.DefaultCargoId,
                IdEstado = 1
            };

            _context.AdmPersonals.Add(personal);
            await _context.SaveChangesAsync();

            usuario.IdPersonal = personal.IdPersonal;
            _context.AdmUsuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Asignar permisos (SubMenus)
            if (dto.SubMenusSeleccionados != null && dto.SubMenusSeleccionados.Any())
            {
                var maxIdDetalle = await _context.AdmDetalleSubMenus.MaxAsync(d => (int?)d.IdDsubmenu) ?? 0;
                var detalles = dto.SubMenusSeleccionados.Select((subMenuId, index) => new AdmDetalleSubMenu
                {
                    IdDsubmenu = maxIdDetalle + index + 1,
                    IdSmenu = subMenuId,
                    IdUsuario = usuario.IdUsuario
                }).ToList();

                _context.AdmDetalleSubMenus.AddRange(detalles);
                await _context.SaveChangesAsync();
            }

            var result = new UsuarioDto
            {
                IdUsuario = usuario.IdUsuario,
                NombreUsuarioUsu = usuario.NombreUsuarioUsu,
                ApellidosNombrePer = personal.ApellidosNombrePer
            };

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UsuarioDto>> UpdateUsuario(int id, UsuarioEdicionDto dto)
        {
            var existente = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .Include(u => u.AdmDetalleSubMenus)
                .FirstOrDefaultAsync(u => u.IdUsuario == id && u.IdArea == AppConfig.DefaultAreaId);

            if (existente == null) return NotFound();

            existente.NombreUsuarioUsu = dto.NombreUsuarioUsu;

            if (existente.IdPersonalNavigation != null)
            {
                var personal = existente.IdPersonalNavigation;
                personal.Paterno = dto.Paterno;
                personal.Materno = dto.Materno;
                personal.Nombre = dto.Nombre;
                personal.ApellidosNombrePer = $"{dto.Paterno} {dto.Materno} {dto.Nombre}".Trim();
            }

            // Actualizar permisos (SubMenus)
            // 1. Eliminar permisos existentes
            if (existente.AdmDetalleSubMenus.Any())
            {
                _context.AdmDetalleSubMenus.RemoveRange(existente.AdmDetalleSubMenus);
            }

            // 2. Agregar nuevos permisos
            if (dto.SubMenusSeleccionados != null && dto.SubMenusSeleccionados.Any())
            {
                var maxIdDetalle = await _context.AdmDetalleSubMenus.MaxAsync(d => (int?)d.IdDsubmenu) ?? 0;
                var detalles = dto.SubMenusSeleccionados.Select((subMenuId, index) => new AdmDetalleSubMenu
                {
                    IdDsubmenu = maxIdDetalle + index + 1,
                    IdSmenu = subMenuId,
                    IdUsuario = existente.IdUsuario
                }).ToList();

                _context.AdmDetalleSubMenus.AddRange(detalles);
            }

            await _context.SaveChangesAsync();

            var result = new UsuarioDto
            {
                IdUsuario = existente.IdUsuario,
                NombreUsuarioUsu = existente.NombreUsuarioUsu,
                ApellidosNombrePer = existente.IdPersonalNavigation?.ApellidosNombrePer
            };

            return result;
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
