using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VisitasTickets.Application.Interfaces;
using VisitasTickets.Infrastructure.Persistence;

namespace VisitasTickets.Application.Services
{
    public class AuthService : IAuthService
    {

        private readonly AppDbContext _context; 
        private readonly IConfiguration _configuration;

        public AuthService(AppDbContext context, IConfiguration configuration) 
        {
            _context = context; _configuration = configuration; 
        }

        public async Task<string?> AuthenticateAsync(string username, string password)
        {
            var usuario = await _context.AdmUsuarios
                .Include(u => u.IdPersonalNavigation)
                .FirstOrDefaultAsync(u => u.NombreUsuarioUsu == username && u.ContrasenaUsu == password);

            if (usuario == null) return null;

            // Obtener nombre completo del personal
            string nombreCompleto = "Usuario";
            if (usuario.IdPersonalNavigation != null)
            {
                var personal = usuario.IdPersonalNavigation;
                nombreCompleto = !string.IsNullOrWhiteSpace(personal.ApellidosNombrePer)
                    ? personal.ApellidosNombrePer
                    : $"{personal.Nombre} {personal.Paterno} {personal.Materno}".Trim();
            }

            var claims = new List<Claim>
            {
                new Claim("UsuarioId", usuario.IdUsuario.ToString()),
                new Claim(ClaimTypes.Name, nombreCompleto)
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

        public async Task<object> GetAccesosAsync(int usuarioId)
        {
            var accesos = await _context.AdmDetalleSubMenus
                .Include(d => d.IdSmenuNavigation)
                    .ThenInclude(s => s.IdMenuNavigation)
                        .ThenInclude(m => m.IdModuloNavigation)
                .Where(d => d.IdUsuario == usuarioId)
                .ToListAsync();

            var resultado = accesos
                .GroupBy(d => d.IdSmenuNavigation.IdMenuNavigation.IdModuloNavigation)
                .Select(modulo => new {
                    idModulo = modulo.Key.IdModulo,
                    nombreModulo = modulo.Key.DescripcionMod,
                    menus = modulo.GroupBy(d => d.IdSmenuNavigation.IdMenuNavigation)
                        .Select(menu => new {
                            idMenu = menu.Key.IdMenu,
                            nombreMenu = menu.Key.DescripcionMen,
                            subMenus = menu.Select(d => new {
                                idSubMenu = d.IdSmenuNavigation.IdSmenu,
                                nombreSubMenu = d.IdSmenuNavigation.DescripcionSme,
                                ruta = d.IdSmenuNavigation.RutaWebSme
                            })
                        })
                });

            return new { usuarioId, modulos = resultado };
        }


    }
}
