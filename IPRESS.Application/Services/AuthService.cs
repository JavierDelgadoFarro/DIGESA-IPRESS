using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IPRESS.Application.Interfaces;
using IPRESS.Domain.Entities;
using IPRESS.Domain.Dtos;
using IPRESS.Infrastructure.Persistence;

namespace IPRESS.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IpressDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(IpressDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.NombreUsuario == username && u.Contrasena == password && u.Activo);

            if (usuario == null) return null;

            var claims = new List<Claim>
            {
                new Claim("UsuarioId", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreCompleto)
            };

            var keyValue = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateToken(int usuarioId, string nombreUsuario, string nombreCompleto)
        {
            var claims = new List<Claim>
            {
                new Claim("UsuarioId", usuarioId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
                new Claim(ClaimTypes.Name, nombreCompleto)
            };
            var keyValue = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyValue));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<object> GetAccesosAsync(int usuarioId)
        {
            var roleIds = await _context.UsuarioRoles
                .Where(ur => ur.IdUsuario == usuarioId)
                .Select(ur => ur.IdRol)
                .ToListAsync();

            var botonesPermitidos = await _context.RolBotones
                .Where(rb => roleIds.Contains(rb.IdRol))
                .Select(rb => rb.Boton)
                .Where(b => b != null)
                .Select(b => new { b!.IdModulo, b.Codigo })
                .ToListAsync();

            var botonesPorModulo = botonesPermitidos
                .GroupBy(x => x.IdModulo)
                .ToDictionary(g => g.Key, g => g.Select(x => x.Codigo).ToList());

            var modulos = await _context.Modulos.ToListAsync();

            // Nueva estructura: RolSubMenu
            var subMenusConAcceso = await _context.RolSubMenus
                .Where(rs => roleIds.Contains(rs.IdRol))
                .Include(rs => rs.SubMenu)
                .ThenInclude(s => s!.Menu)
                .ThenInclude(m => m!.Modulo)
                .Select(rs => rs.SubMenu)
                .Where(s => s != null && s.Menu != null && s.Menu.Modulo != null)
                .Distinct()
                .ToListAsync();

            if (subMenusConAcceso.Any())
            {
                var smList = subMenusConAcceso.Cast<IpressSubMenu>().OrderBy(s => s.Menu!.Orden).ThenBy(s => s.Orden).ToList();
                var modulosAgrupados = smList.GroupBy(s => s.Menu!.Modulo!.IdModulo);
                var modulosLista = modulosAgrupados.OrderBy(g => g.Key).Select(g =>
                {
                    var mod = g.First().Menu!.Modulo!;
                    var menusAgrup = g.GroupBy(s => s.Menu!.IdMenu).OrderBy(gr => gr.First().Menu!.Orden);
                    var menus = menusAgrup.Select(mg =>
                    {
                        var menu = mg.First().Menu!;
                        var subMenus = mg.OrderBy(s => s.Orden).Select(s =>
                        {
                            var modBoton = modulos.FirstOrDefault(m => m.Codigo == s.Codigo) ?? modulos.FirstOrDefault(m => m.Ruta == s.Ruta) ?? mod;
                            var botones = botonesPorModulo.GetValueOrDefault(modBoton.IdModulo, new List<string>());
                            return new SubMenu
                            {
                                IdSubMenu = s.IdSubMenu,
                                NombreSubMenu = s.Nombre,
                                Ruta = s.Ruta,
                                Icono = s.Icono,
                                Botones = botones
                            };
                        }).ToList();
                        return new Menu
                        {
                            IdMenu = menu.IdMenu,
                            NombreMenu = menu.Nombre,
                            SubMenus = subMenus
                        };
                    }).ToList();
                    return new Modulo
                    {
                        IdModulo = mod.IdModulo,
                        NombreModulo = mod.Nombre,
                        Menus = menus
                    };
                }).ToList();

                return new AccesosResponse { UsuarioId = usuarioId, Modulos = modulosLista };
            }

            // Fallback: RolModulo (estructura antigua)
            var modsLegacy = await _context.RolModulos
                .Where(rm => roleIds.Contains(rm.IdRol))
                .Select(rm => rm.Modulo)
                .Where(m => m != null)
                .Distinct()
                .OrderBy(m => m!.Orden)
                .Cast<IpressModulo>()
                .ToListAsync();

            var modulosLegacy = modsLegacy.Select(m => new Modulo
            {
                IdModulo = m!.IdModulo,
                NombreModulo = m.Nombre,
                Menus = new List<Menu>
                {
                    new Menu
                    {
                        IdMenu = m.IdModulo,
                        NombreMenu = "Maestros",
                        SubMenus = new List<SubMenu>
                        {
                            new SubMenu
                            {
                                IdSubMenu = m.IdModulo,
                                NombreSubMenu = m.Nombre,
                                Ruta = m.Ruta ?? "",
                                Icono = null,
                                Botones = botonesPorModulo.GetValueOrDefault(m.IdModulo, new List<string>())
                            }
                        }
                    }
                }
            }).ToList();

            return new AccesosResponse { UsuarioId = usuarioId, Modulos = modulosLegacy };
        }
    }
}
