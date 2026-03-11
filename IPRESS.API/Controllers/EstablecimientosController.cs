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
    public class EstablecimientosController : ControllerBase
    {
        private readonly IpressDbContext _context;

        public EstablecimientosController(IpressDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] int? idMicroRed)
        {
            var q = _context.Establecimientos.AsNoTracking()
                .Include(e => e.Diresa).Include(e => e.Red).Include(e => e.MicroRed)
                .Where(e => e.Activo);
            if (idMicroRed.HasValue)
                q = q.Where(e => e.IdMicroRed == idMicroRed.Value);
            return Ok(await q.OrderBy(e => e.Nombre)
                .Select(e => new
                {
                    e.IdEstablecimiento,
                    Diresa = e.Diresa != null ? e.Diresa.Nombre : "",
                    Red = e.Red != null ? e.Red.Nombre : "",
                    MicroRed = e.MicroRed != null ? e.MicroRed.Nombre : "",
                    e.QuintilRegional,
                    e.Ubigeo,
                    e.Codigo,
                    e.Nombre
                }).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> Get(int id)
        {
            var e = await _context.Establecimientos.AsNoTracking()
                .Include(x => x.Diresa).Include(x => x.Red).Include(x => x.MicroRed)
                .Include(x => x.EstablecimientoCentrosPoblados)
                    .ThenInclude(ec => ec.CentroPoblado)
                .FirstOrDefaultAsync(x => x.IdEstablecimiento == id);
            if (e == null) return NotFound();
            return Ok(new
            {
                e.IdEstablecimiento,
                e.Codigo,
                e.Nombre,
                e.Departamento,
                e.Provincia,
                e.Distrito,
                e.QuintilRegional,
                e.Ubigeo,
                e.AltitudMsnm,
                e.IdDiresa,
                e.IdRed,
                e.IdMicroRed,
                e.TieneTelefono,
                e.TieneRadio,
                e.Este,
                e.Norte,
                e.Huso,
                e.Banda,
                e.Latitud,
                e.Longitud,
                CentrosPoblados = e.EstablecimientoCentrosPoblados.Select(ec => new
                {
                    ec.CentroPoblado!.IdCentroPoblado,
                    Ubigeo = ec.CentroPoblado.Ubigeo ?? ec.CentroPoblado.UbigeoCcp,
                    ec.CentroPoblado.UbigeoCcp,
                    ec.CentroPoblado.Departamento,
                    ec.CentroPoblado.Provincia,
                    ec.CentroPoblado.Distrito,
                    ec.CentroPoblado.CentroPoblado,
                    ec.CentroPoblado.Ambito,
                    ec.CentroPoblado.Quintil
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<IpressEstablecimiento>> Post(EstablecimientoInput input)
        {
            if (await _context.Establecimientos.AnyAsync(x => x.Codigo == input.Codigo))
                return BadRequest(new { message = "El código ya existe." });
            var e = MapToEntity(input);
            _context.Establecimientos.Add(e);
            await _context.SaveChangesAsync();
            await ActualizarCentrosPoblados(e.IdEstablecimiento, input.CentrosPobladoIds);
            return CreatedAtAction(nameof(Get), new { id = e.IdEstablecimiento }, e);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, EstablecimientoInput input)
        {
            var e = await _context.Establecimientos.FindAsync(id);
            if (e == null) return NotFound();
            MapToEntity(input, e);
            await _context.SaveChangesAsync();
            await ActualizarCentrosPoblados(id, input.CentrosPobladoIds);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var e = await _context.Establecimientos.FindAsync(id);
            if (e == null) return NotFound();
            e.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static IpressEstablecimiento MapToEntity(EstablecimientoInput input, IpressEstablecimiento? e = null)
        {
            e ??= new IpressEstablecimiento();
            e.Codigo = input.Codigo;
            e.Nombre = input.Nombre;
            e.Departamento = input.Departamento;
            e.Provincia = input.Provincia;
            e.Distrito = input.Distrito;
            e.QuintilRegional = input.QuintilRegional;
            e.Ubigeo = input.Ubigeo;
            e.AltitudMsnm = input.AltitudMsnm;
            e.IdDiresa = input.IdDiresa is 0 ? null : input.IdDiresa;
            e.IdRed = input.IdRed is 0 ? null : input.IdRed;
            e.IdMicroRed = input.IdMicroRed is 0 ? null : input.IdMicroRed;
            e.TieneTelefono = input.TieneTelefono;
            e.TieneRadio = input.TieneRadio;
            e.Este = input.Este;
            e.Norte = input.Norte;
            e.Huso = input.Huso;
            e.Banda = input.Banda;
            e.Latitud = input.Latitud;
            e.Longitud = input.Longitud;
            return e;
        }

        private async Task ActualizarCentrosPoblados(int idEstab, List<int>? ids)
        {
            var actuales = await _context.EstablecimientoCentrosPoblados
                .Where(ec => ec.IdEstablecimiento == idEstab).ToListAsync();
            _context.EstablecimientoCentrosPoblados.RemoveRange(actuales);
            if (ids != null)
            {
                foreach (var idCp in ids)
                {
                    _context.EstablecimientoCentrosPoblados.Add(new IpressEstablecimientoCentroPoblado
                    {
                        IdEstablecimiento = idEstab,
                        IdCentroPoblado = idCp
                    });
                }
            }
            await _context.SaveChangesAsync();
        }
    }

    public class EstablecimientoInput
    {
        public int IdEstablecimiento { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? QuintilRegional { get; set; }
        public string? Ubigeo { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public bool TieneTelefono { get; set; }
        public bool TieneRadio { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public List<int>? CentrosPobladoIds { get; set; }
    }
}
