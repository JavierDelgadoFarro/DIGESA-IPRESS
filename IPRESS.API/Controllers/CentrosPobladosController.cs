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
    public class CentrosPobladosController : ControllerBase
    {
        private readonly IpressDbContext _context;

        public CentrosPobladosController(IpressDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> Get([FromQuery] string? search)
        {
            var q = _context.CentrosPoblados.AsNoTracking()
                .Include(c => c.Establecimiento)
                .Where(c => c.Activo);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                q = q.Where(c =>
                    (c.CentroPoblado != null && c.CentroPoblado.ToLower().Contains(s)) ||
                    (c.UbigeoCcp != null && c.UbigeoCcp.Contains(s)) ||
                    (c.Distrito != null && c.Distrito.ToLower().Contains(s)) ||
                    (c.Provincia != null && c.Provincia.ToLower().Contains(s)));
            }
            return Ok(await q.OrderBy(c => c.CentroPoblado)
                .Select(c => new
                {
                    c.IdCentroPoblado,
                    c.Ubigeo,
                    c.UbigeoCcp,
                    c.Departamento,
                    c.Provincia,
                    c.Distrito,
                    c.CentroPoblado,
                    c.IdEstablecimiento,
                    EstablecimientoNombre = c.Establecimiento != null ? c.Establecimiento.Nombre : (string?)null,
                    c.Ambito,
                    c.Quintil
                }).Take(500).ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<object>> GetById(int id)
        {
            var c = await _context.CentrosPoblados.AsNoTracking()
                .Include(x => x.Establecimiento)
                .Include(x => x.Accesibilidades)
                .Include(x => x.CentrosEducativos)
                .Include(x => x.Autoridades)
                .FirstOrDefaultAsync(x => x.IdCentroPoblado == id);
            if (c == null) return NotFound();
            return Ok(new
            {
                c.IdCentroPoblado,
                c.Ubigeo,
                c.UbigeoCcp,
                c.Departamento,
                c.Provincia,
                c.Distrito,
                c.CentroPoblado,
                c.IdEstablecimiento,
                Establecimiento = c.Establecimiento == null ? null : new { c.Establecimiento.IdEstablecimiento, c.Establecimiento.Codigo, c.Establecimiento.Nombre, c.Establecimiento.Departamento, c.Establecimiento.Provincia, c.Establecimiento.Distrito, c.Establecimiento.Ubigeo },
                c.Ambito,
                c.Quintil,
                c.Este,
                c.Norte,
                c.Huso,
                c.Banda,
                c.Latitud,
                c.Longitud,
                c.AltitudMsnm,
                c.PoblacionTotal,
                c.PoblacionServida,
                c.PoblacionVigilada,
                c.ElectricidadHrs,
                c.TelefonoTipo,
                c.TelefonoNumero,
                c.RadioEmisora,
                c.RadioESS,
                c.SenalTV,
                c.Internet,
                c.LimpiezaPublica,
                c.Agua,
                c.Letrinas,
                c.DesagueAlcantarillado,
                c.SistEliminacionExcretas,
                c.Vertimientos,
                c.TempMinima,
                c.TempMaxima,
                Accesibilidades = c.Accesibilidades.Select(a => new { a.IdAccesibilidad, a.Desde, a.Hasta, a.DistanciaKm, a.TiempoMin, a.TipoVia, a.MedioTransporte }).ToList(),
                CentrosEducativos = c.CentrosEducativos.Select(ce => new { ce.IdCentroEducativo, ce.TipoCentroEducativo, ce.NombreCentroEducativo }).ToList(),
                Autoridades = c.Autoridades.Select(a => new { a.IdAutoridad, a.TipoAutoridad, a.NombreAutoridad }).ToList()
            });
        }

        [HttpPost]
        public async Task<ActionResult<object>> Post([FromBody] CentroPobladoRequest body)
        {
            if (body == null || string.IsNullOrWhiteSpace(body.CentroPoblado))
                return BadRequest(new { message = "Nombre del centro poblado es requerido." });
            var c = MapToEntity(body, new IpressCentroPoblado());
            _context.CentrosPoblados.Add(c);
            await _context.SaveChangesAsync();
            await SaveChildren(c.IdCentroPoblado, body);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = c.IdCentroPoblado }, new { c.IdCentroPoblado, c.CentroPoblado });
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Put(int id, [FromBody] CentroPobladoRequest body)
        {
            var c = await _context.CentrosPoblados
                .Include(x => x.Accesibilidades)
                .Include(x => x.CentrosEducativos)
                .Include(x => x.Autoridades)
                .FirstOrDefaultAsync(x => x.IdCentroPoblado == id);
            if (c == null) return NotFound();
            MapToEntity(body, c);
            _context.CentrosPoblados.Update(c);
            _context.CentroPobladoAccesibilidades.RemoveRange(c.Accesibilidades);
            _context.CentroPobladoCentrosEducativos.RemoveRange(c.CentrosEducativos);
            _context.CentroPobladoAutoridades.RemoveRange(c.Autoridades);
            await _context.SaveChangesAsync();
            await SaveChildren(id, body);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var c = await _context.CentrosPoblados.FindAsync(id);
            if (c == null) return NotFound();
            c.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static IpressCentroPoblado MapToEntity(CentroPobladoRequest body, IpressCentroPoblado c)
        {
            c.Ubigeo = body.Ubigeo;
            c.UbigeoCcp = body.UbigeoCcp ?? "";
            c.Departamento = body.Departamento;
            c.Provincia = body.Provincia;
            c.Distrito = body.Distrito;
            c.CentroPoblado = body.CentroPoblado;
            c.IdEstablecimiento = body.IdEstablecimiento;
            c.Ambito = body.Ambito;
            c.Quintil = body.Quintil;
            c.Este = body.Este;
            c.Norte = body.Norte;
            c.Huso = body.Huso;
            c.Banda = body.Banda;
            c.Latitud = body.Latitud;
            c.Longitud = body.Longitud;
            c.AltitudMsnm = body.AltitudMsnm;
            c.PoblacionTotal = body.PoblacionTotal;
            c.PoblacionServida = body.PoblacionServida;
            c.PoblacionVigilada = body.PoblacionVigilada;
            c.ElectricidadHrs = body.ElectricidadHrs;
            c.TelefonoTipo = body.TelefonoTipo;
            c.TelefonoNumero = body.TelefonoNumero;
            c.RadioEmisora = body.RadioEmisora;
            c.RadioESS = body.RadioESS;
            c.SenalTV = body.SenalTV;
            c.Internet = body.Internet;
            c.LimpiezaPublica = body.LimpiezaPublica;
            c.Agua = body.Agua;
            c.Letrinas = body.Letrinas;
            c.DesagueAlcantarillado = body.DesagueAlcantarillado;
            c.SistEliminacionExcretas = body.SistEliminacionExcretas;
            c.Vertimientos = body.Vertimientos;
            c.TempMinima = body.TempMinima;
            c.TempMaxima = body.TempMaxima;
            return c;
        }

        private async Task SaveChildren(int idCentroPoblado, CentroPobladoRequest body)
        {
            if (body.Accesibilidades != null)
            {
                foreach (var a in body.Accesibilidades)
                {
                    _context.CentroPobladoAccesibilidades.Add(new IpressCentroPobladoAccesibilidad
                    {
                        IdCentroPoblado = idCentroPoblado,
                        Desde = a.Desde,
                        Hasta = a.Hasta,
                        DistanciaKm = a.DistanciaKm,
                        TiempoMin = a.TiempoMin,
                        TipoVia = a.TipoVia,
                        MedioTransporte = a.MedioTransporte
                    });
                }
            }
            if (body.CentrosEducativos != null)
            {
                foreach (var ce in body.CentrosEducativos)
                {
                    _context.CentroPobladoCentrosEducativos.Add(new IpressCentroPobladoCentroEducativo
                    {
                        IdCentroPoblado = idCentroPoblado,
                        TipoCentroEducativo = ce.TipoCentroEducativo,
                        NombreCentroEducativo = ce.NombreCentroEducativo
                    });
                }
            }
            if (body.Autoridades != null)
            {
                foreach (var a in body.Autoridades)
                {
                    _context.CentroPobladoAutoridades.Add(new IpressCentroPobladoAutoridad
                    {
                        IdCentroPoblado = idCentroPoblado,
                        TipoAutoridad = a.TipoAutoridad,
                        NombreAutoridad = a.NombreAutoridad
                    });
                }
            }
            await Task.CompletedTask;
        }
    }

    public class CentroPobladoRequest
    {
        public string? Ubigeo { get; set; }
        public string? UbigeoCcp { get; set; }
        public string? Departamento { get; set; }
        public string? Provincia { get; set; }
        public string? Distrito { get; set; }
        public string? CentroPoblado { get; set; }
        public int? IdEstablecimiento { get; set; }
        public string? Ambito { get; set; }
        public string? Quintil { get; set; }
        public decimal? Este { get; set; }
        public decimal? Norte { get; set; }
        public int? Huso { get; set; }
        public string? Banda { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public int? AltitudMsnm { get; set; }
        public int? PoblacionTotal { get; set; }
        public int? PoblacionServida { get; set; }
        public int? PoblacionVigilada { get; set; }
        public int? ElectricidadHrs { get; set; }
        public string? TelefonoTipo { get; set; }
        public string? TelefonoNumero { get; set; }
        public bool RadioEmisora { get; set; }
        public bool RadioESS { get; set; }
        public bool SenalTV { get; set; }
        public bool Internet { get; set; }
        public bool LimpiezaPublica { get; set; }
        public bool Agua { get; set; }
        public bool Letrinas { get; set; }
        public bool DesagueAlcantarillado { get; set; }
        public bool SistEliminacionExcretas { get; set; }
        public string? Vertimientos { get; set; }
        public decimal? TempMinima { get; set; }
        public decimal? TempMaxima { get; set; }
        public List<AccesibilidadItem>? Accesibilidades { get; set; }
        public List<CentroEducativoItem>? CentrosEducativos { get; set; }
        public List<AutoridadItem>? Autoridades { get; set; }
    }

    public class AccesibilidadItem
    {
        public string? Desde { get; set; }
        public string? Hasta { get; set; }
        public decimal? DistanciaKm { get; set; }
        public int? TiempoMin { get; set; }
        public string? TipoVia { get; set; }
        public string? MedioTransporte { get; set; }
    }

    public class CentroEducativoItem
    {
        public string? TipoCentroEducativo { get; set; }
        public string? NombreCentroEducativo { get; set; }
    }

    public class AutoridadItem
    {
        public string? TipoAutoridad { get; set; }
        public string? NombreAutoridad { get; set; }
    }
}
