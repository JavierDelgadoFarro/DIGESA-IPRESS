using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IPRESS.Infrastructure.Persistence;
using IPRESS.Domain.Entities;

namespace IPRESS.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GestionUsuariosController : ControllerBase
    {
        private readonly IpressDbContext _context;

        public GestionUsuariosController(IpressDbContext context) => _context = context;

        [HttpGet("usuarios")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsuarios()
        {
            var usuarios = await _context.Usuarios.AsNoTracking()
                .Include(u => u.UsuarioRoles)
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();
            var idDiresas = usuarios.Where(u => u.IdDiresa.HasValue).Select(u => u.IdDiresa!.Value).Distinct().ToList();
            var idRedes = usuarios.Where(u => u.IdRed.HasValue).Select(u => u.IdRed!.Value).Distinct().ToList();
            var idMicroRedes = usuarios.Where(u => u.IdMicroRed.HasValue).Select(u => u.IdMicroRed!.Value).Distinct().ToList();
            var idEstab = usuarios.Where(u => u.IdEstablecimiento.HasValue).Select(u => u.IdEstablecimiento!.Value).Distinct().ToList();
            var taskDiresas = idDiresas.Count > 0 ? _context.Diresas.AsNoTracking().Where(x => idDiresas.Contains(x.IdDiresa)).ToDictionaryAsync(x => x.IdDiresa, x => x.Nombre) : Task.FromResult(new Dictionary<int, string>());
            var taskRedes = idRedes.Count > 0 ? _context.Redes.AsNoTracking().Where(r => idRedes.Contains(r.IdRed)).ToDictionaryAsync(r => r.IdRed, r => r.Nombre) : Task.FromResult(new Dictionary<int, string>());
            var taskMicroRedes = idMicroRedes.Count > 0 ? _context.MicroRedes.AsNoTracking().Where(m => idMicroRedes.Contains(m.IdMicroRed)).ToDictionaryAsync(m => m.IdMicroRed, m => m.Nombre) : Task.FromResult(new Dictionary<int, string>());
            var taskEstab = idEstab.Count > 0 ? _context.Establecimientos.AsNoTracking().Where(e => idEstab.Contains(e.IdEstablecimiento)).ToDictionaryAsync(e => e.IdEstablecimiento, e => e.Nombre) : Task.FromResult(new Dictionary<int, string>());
            await Task.WhenAll(taskDiresas, taskRedes, taskMicroRedes, taskEstab);
            var diresas = await taskDiresas;
            var redes = await taskRedes;
            var microRedes = await taskMicroRedes;
            var establecimientos = await taskEstab;
            var list = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.NombreUsuario,
                u.NombreCompleto,
                u.Email,
                u.Activo,
                u.IdDiresa,
                u.IdRed,
                u.IdMicroRed,
                u.IdEstablecimiento,
                DiresaNombre = u.IdDiresa.HasValue && diresas.TryGetValue(u.IdDiresa.Value, out var dn) ? dn : (string?)null,
                RedNombre = u.IdRed.HasValue && redes.TryGetValue(u.IdRed.Value, out var rn) ? rn : (string?)null,
                MicroRedNombre = u.IdMicroRed.HasValue && microRedes.TryGetValue(u.IdMicroRed.Value, out var mn) ? mn : (string?)null,
                EstablecimientoNombre = u.IdEstablecimiento.HasValue && establecimientos.TryGetValue(u.IdEstablecimiento.Value, out var en) ? en : (string?)null,
                Roles = u.UsuarioRoles.Select(ur => ur.IdRol).ToList()
            }).ToList();
            return Ok(list);
        }

        [HttpGet("usuarios/{id:int}")]
        public async Task<ActionResult<object>> GetUsuario(int id)
        {
            var u = await _context.Usuarios.AsNoTracking()
                .Where(x => x.IdUsuario == id)
                .Select(x => new { x.IdUsuario, x.NombreUsuario, x.NombreCompleto, x.Email, x.Activo, x.IdDiresa, x.IdRed, x.IdMicroRed, x.IdEstablecimiento, RolIds = x.UsuarioRoles.Select(ur => ur.IdRol).ToList() })
                .FirstOrDefaultAsync();
            return u == null ? NotFound() : Ok(u);
        }

        [HttpPost("usuarios")]
        public async Task<IActionResult> PostUsuario([FromBody] UsuarioCreateRequest body)
        {
            if (string.IsNullOrWhiteSpace(body.NombreUsuario) || string.IsNullOrWhiteSpace(body.NombreCompleto))
                return BadRequest(new { message = "NombreUsuario y NombreCompleto son requeridos." });
            if (await _context.Usuarios.AnyAsync(x => x.NombreUsuario == body.NombreUsuario.Trim()))
                return BadRequest(new { message = "El nombre de usuario ya existe." });
            var contrasena = string.IsNullOrWhiteSpace(body.Contrasena) ? body.NombreUsuario : body.Contrasena;
            var usuario = new IpressUsuario
            {
                NombreUsuario = body.NombreUsuario.Trim(),
                Contrasena = BCrypt.Net.BCrypt.HashPassword(contrasena),
                NombreCompleto = body.NombreCompleto.Trim(),
                Email = body.Email?.Trim(),
                Activo = body.Activo,
                IdDiresa = body.IdDiresa,
                IdRed = body.IdRed,
                IdMicroRed = body.IdMicroRed,
                IdEstablecimiento = body.IdEstablecimiento
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();
            foreach (var idRol in body.RolIds ?? new List<int>())
            {
                _context.UsuarioRoles.Add(new IpressUsuarioRol { IdUsuario = usuario.IdUsuario, IdRol = idRol });
            }
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, new { usuario.IdUsuario, usuario.NombreUsuario, usuario.NombreCompleto });
        }

        [HttpPut("usuarios/{id:int}")]
        public async Task<IActionResult> PutUsuario(int id, [FromBody] UsuarioUpdateRequest body)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            if (string.IsNullOrWhiteSpace(body.NombreCompleto)) return BadRequest(new { message = "NombreCompleto es requerido." });
            usuario.NombreCompleto = body.NombreCompleto.Trim();
            usuario.Email = body.Email?.Trim();
            usuario.Activo = body.Activo;
            usuario.IdDiresa = body.IdDiresa;
            usuario.IdRed = body.IdRed;
            usuario.IdMicroRed = body.IdMicroRed;
            usuario.IdEstablecimiento = body.IdEstablecimiento;
            if (!string.IsNullOrWhiteSpace(body.Contrasena))
                usuario.Contrasena = BCrypt.Net.BCrypt.HashPassword(body.Contrasena);
            await _context.SaveChangesAsync();
            var actuales = await _context.UsuarioRoles.Where(ur => ur.IdUsuario == id).ToListAsync();
            _context.UsuarioRoles.RemoveRange(actuales);
            foreach (var idRol in body.RolIds ?? new List<int>())
                _context.UsuarioRoles.Add(new IpressUsuarioRol { IdUsuario = id, IdRol = idRol });
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("usuarios/{id:int}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            var roles = await _context.UsuarioRoles.Where(ur => ur.IdUsuario == id).ToListAsync();
            _context.UsuarioRoles.RemoveRange(roles);
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<object>>> GetRoles()
        {
            var list = await _context.Roles.AsNoTracking()
                .Select(r => new { r.IdRol, r.Codigo, r.Nombre })
                .OrderBy(r => r.Nombre)
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost("roles")]
        public async Task<ActionResult<object>> PostRol([FromBody] RolCreateRequest body)
        {
            if (string.IsNullOrWhiteSpace(body?.Codigo) || string.IsNullOrWhiteSpace(body?.Nombre))
                return BadRequest(new { message = "Código y Nombre son requeridos." });
            if (await _context.Roles.AnyAsync(r => r.Codigo == body.Codigo.Trim()))
                return BadRequest(new { message = "Ya existe un rol con ese código." });
            var rol = new IpressRol { Codigo = body.Codigo.Trim(), Nombre = body.Nombre.Trim() };
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetRoles), new { id = rol.IdRol }, new { rol.IdRol, rol.Codigo, rol.Nombre });
        }

        [HttpPut("roles/{id:int}")]
        public async Task<IActionResult> PutRol(int id, [FromBody] RolUpdateRequest body)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            if (string.IsNullOrWhiteSpace(body?.Nombre))
                return BadRequest(new { message = "Nombre es requerido." });
            rol.Nombre = body.Nombre.Trim();
            if (!string.IsNullOrWhiteSpace(body.Codigo))
                rol.Codigo = body.Codigo.Trim();
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("roles/{id:int}")]
        public async Task<IActionResult> DeleteRol(int id)
        {
            var rol = await _context.Roles.FindAsync(id);
            if (rol == null) return NotFound();
            var tieneUsuarios = await _context.UsuarioRoles.AnyAsync(ur => ur.IdRol == id);
            if (tieneUsuarios)
                return BadRequest(new { message = "No se puede eliminar el rol porque tiene usuarios asignados." });
            _context.RolSubMenus.RemoveRange(await _context.RolSubMenus.Where(rs => rs.IdRol == id).ToListAsync());
            _context.RolBotones.RemoveRange(await _context.RolBotones.Where(rb => rb.IdRol == id).ToListAsync());
            _context.RolModulos.RemoveRange(await _context.RolModulos.Where(rm => rm.IdRol == id).ToListAsync());
            _context.Roles.Remove(rol);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("estructura")]
        public async Task<ActionResult<object>> GetEstructura()
        {
            var modulos = await _context.Modulos.AsNoTracking().OrderBy(m => m.Orden).ToListAsync();
            var menus = await _context.Menus.AsNoTracking().OrderBy(m => m.Orden).ToListAsync();
            var submenus = await _context.SubMenus.AsNoTracking().OrderBy(s => s.Orden).ToListAsync();
            var botones = await _context.Botones.AsNoTracking().ToListAsync();
            var moduloIds = modulos.Select(m => m.IdModulo).ToHashSet();
            var menusFiltrados = menus.Where(me => moduloIds.Contains(me.IdModulo)).ToList();
            var menuIds = menusFiltrados.Select(me => me.IdMenu).ToHashSet();
            var submenusFiltrados = submenus.Where(s => menuIds.Contains(s.IdMenu)).ToList();
            var botonesFiltrados = botones.Where(b => moduloIds.Contains(b.IdModulo)).ToList();
            var estructura = modulos.Select(m => new
            {
                m.IdModulo,
                m.Codigo,
                m.Nombre,
                Menus = menusFiltrados.Where(me => me.IdModulo == m.IdModulo).Select(me => new
                {
                    me.IdMenu,
                    me.Nombre,
                    SubMenus = submenusFiltrados.Where(s => s.IdMenu == me.IdMenu).Select(s => new { s.IdSubMenu, s.Nombre, s.Ruta }).ToList()
                }).ToList()
            }).ToList();
            var botonesPorModulo = botonesFiltrados.Select(b => new { b.IdBoton, b.IdModulo, b.Codigo, b.Nombre }).ToList();
            return Ok(new { estructura, botonesPorModulo });
        }

        [HttpGet("roles/{idRol:int}/permisos")]
        public async Task<ActionResult<object>> GetPermisosRol(int idRol)
        {
            var subMenuIds = await _context.RolSubMenus.Where(rs => rs.IdRol == idRol).Select(rs => rs.IdSubMenu).ToListAsync();
            var botonIds = await _context.RolBotones.Where(rb => rb.IdRol == idRol).Select(rb => rb.IdBoton).ToListAsync();
            return Ok(new { subMenuIds, botonIds });
        }

        /// <summary>Obtiene permisos fusionados (sin duplicados) para uno o más roles. Útil para mostrar qué permisos tiene un usuario por la suma de sus roles.</summary>
        [HttpPost("permisos-por-roles")]
        public async Task<ActionResult<object>> GetPermisosPorRoles([FromBody] RolIdsRequest body)
        {
            var ids = body?.RolIds?.Distinct().ToList() ?? new List<int>();
            if (ids.Count == 0)
                return Ok(new { subMenuIds = new List<int>(), botonIds = new List<int>() });
            var subMenuIds = await _context.RolSubMenus.Where(rs => ids.Contains(rs.IdRol)).Select(rs => rs.IdSubMenu).Distinct().ToListAsync();
            var botonIds = await _context.RolBotones.Where(rb => ids.Contains(rb.IdRol)).Select(rb => rb.IdBoton).Distinct().ToListAsync();
            return Ok(new { subMenuIds, botonIds });
        }

        [HttpPut("roles/{idRol:int}/permisos")]
        public async Task<IActionResult> PutPermisosRol(int idRol, [FromBody] PermisosRolRequest body)
        {
            var exist = await _context.Roles.FindAsync(idRol);
            if (exist == null) return NotFound();
            var actualesSub = await _context.RolSubMenus.Where(rs => rs.IdRol == idRol).ToListAsync();
            var actualesBot = await _context.RolBotones.Where(rb => rb.IdRol == idRol).ToListAsync();
            _context.RolSubMenus.RemoveRange(actualesSub);
            _context.RolBotones.RemoveRange(actualesBot);
            foreach (var id in body.SubMenuIds ?? new List<int>())
                _context.RolSubMenus.Add(new IpressRolSubMenu { IdRol = idRol, IdSubMenu = id });
            foreach (var id in body.BotonIds ?? new List<int>())
                _context.RolBotones.Add(new IpressRolBoton { IdRol = idRol, IdBoton = id });
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
        }
    }

    public class UsuarioCreateRequest
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

    public class UsuarioUpdateRequest
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

    public class RolIdsRequest
    {
        public List<int>? RolIds { get; set; }
    }

    public class PermisosRolRequest
    {
        public List<int>? SubMenuIds { get; set; }
        public List<int>? BotonIds { get; set; }
    }

    public class RolCreateRequest
    {
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class RolUpdateRequest
    {
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = "";
    }
}
