using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Entities;
using VisitasTickets.Infrastructure.Persistence;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AtencionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AtencionesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/atenciones - Obtener todas las atenciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtenciones()
        {
            var atenciones = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Include(a => a.IdUsuarioActualizaNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .OrderByDescending(a => a.FechaRegistro)
                .Select(a => new AtencionDto
                {
                    IdAtencion = a.IdAtencion,
                    TipoDocumento = a.TipoDocumento,
                    NumeroDocumento = a.NumeroDocumento,
                    Nombres = a.Nombres,
                    Apellidos = a.Apellidos,
                    IdTipoTramite = a.IdTipoTramite,
                    NombreTramite = a.IdTipoTramiteNavigation.NombreTramite,
                    DescripcionTramite = a.IdTipoTramiteNavigation.Descripcion,
                    Observacion = a.Observacion,
                    ObservacionAtencion = a.ObservacionAtencion,
                    EsPreferencial = a.EsPreferencial,
                    IdTipoPreferencial = a.IdTipoPreferencial,
                    NombreTipoPreferencial = a.IdTipoPreferencialNavigation != null ? a.IdTipoPreferencialNavigation.NombreTipoPreferencial : null,
                    IdEstadoAtencion = a.IdEstadoAtencion,
                    NombreEstado = a.IdEstadoAtencionNavigation.NombreEstado,
                    OrdenEstado = a.IdEstadoAtencionNavigation.Orden,
                    FechaRegistro = a.FechaRegistro,
                    FechaActualizacion = a.FechaActualizacion,
                    IdUsuarioRegistro = a.IdUsuarioRegistro,
                    NombreUsuarioRegistro = a.IdUsuarioRegistroNavigation != null ? a.IdUsuarioRegistroNavigation.IdPersonalNavigation!.ApellidosNombrePer : null,
                    IdUsuarioActualiza = a.IdUsuarioActualiza,
                    NombreUsuarioActualiza = a.IdUsuarioActualizaNavigation != null ? a.IdUsuarioActualizaNavigation.IdPersonalNavigation!.ApellidosNombrePer : null
                })
                .ToListAsync();

            return Ok(atenciones);
        }

        // GET: api/atenciones/activas - Obtener atenciones pendientes y en ventanilla
        [HttpGet("activas")]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtencionesActivas()
        {
            var atenciones = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Where(a => a.IdEstadoAtencionNavigation.Orden < 3) // Pendiente y En Ventanilla
                .OrderBy(a => a.FechaRegistro)
                .Select(a => new AtencionDto
                {
                    IdAtencion = a.IdAtencion,
                    TipoDocumento = a.TipoDocumento,
                    NumeroDocumento = a.NumeroDocumento,
                    Nombres = a.Nombres,
                    Apellidos = a.Apellidos,
                    IdTipoTramite = a.IdTipoTramite,
                    NombreTramite = a.IdTipoTramiteNavigation.NombreTramite,
                    DescripcionTramite = a.IdTipoTramiteNavigation.Descripcion,
                    Observacion = a.Observacion,
                    ObservacionAtencion = a.ObservacionAtencion,
                    EsPreferencial = a.EsPreferencial,
                    IdTipoPreferencial = a.IdTipoPreferencial,
                    NombreTipoPreferencial = a.IdTipoPreferencialNavigation != null ? a.IdTipoPreferencialNavigation.NombreTipoPreferencial : null,
                    IdEstadoAtencion = a.IdEstadoAtencion,
                    NombreEstado = a.IdEstadoAtencionNavigation.NombreEstado,
                    OrdenEstado = a.IdEstadoAtencionNavigation.Orden,
                    FechaRegistro = a.FechaRegistro,
                    FechaActualizacion = a.FechaActualizacion,
                    IdUsuarioRegistro = a.IdUsuarioRegistro,
                    NombreUsuarioRegistro = a.IdUsuarioRegistroNavigation != null ? a.IdUsuarioRegistroNavigation.IdPersonalNavigation!.ApellidosNombrePer : null
                })
                .ToListAsync();

            return Ok(atenciones);
        }

        // GET: api/atenciones/historial - Obtener atenciones atendidas
        [HttpGet("historial")]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtencionesHistorial()
        {
            var atenciones = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Include(a => a.IdUsuarioActualizaNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Where(a => a.IdEstadoAtencionNavigation.Orden == 3) // Atendido
                .OrderByDescending(a => a.FechaActualizacion)
                .Select(a => new AtencionDto
                {
                    IdAtencion = a.IdAtencion,
                    TipoDocumento = a.TipoDocumento,
                    NumeroDocumento = a.NumeroDocumento,
                    Nombres = a.Nombres,
                    Apellidos = a.Apellidos,
                    IdTipoTramite = a.IdTipoTramite,
                    NombreTramite = a.IdTipoTramiteNavigation.NombreTramite,
                    DescripcionTramite = a.IdTipoTramiteNavigation.Descripcion,
                    Observacion = a.Observacion,
                    ObservacionAtencion = a.ObservacionAtencion,
                    EsPreferencial = a.EsPreferencial,
                    IdTipoPreferencial = a.IdTipoPreferencial,
                    NombreTipoPreferencial = a.IdTipoPreferencialNavigation != null ? a.IdTipoPreferencialNavigation.NombreTipoPreferencial : null,
                    IdEstadoAtencion = a.IdEstadoAtencion,
                    NombreEstado = a.IdEstadoAtencionNavigation.NombreEstado,
                    OrdenEstado = a.IdEstadoAtencionNavigation.Orden,
                    FechaRegistro = a.FechaRegistro,
                    FechaActualizacion = a.FechaActualizacion,
                    IdUsuarioRegistro = a.IdUsuarioRegistro,
                    NombreUsuarioRegistro = a.IdUsuarioRegistroNavigation != null ? a.IdUsuarioRegistroNavigation.IdPersonalNavigation!.ApellidosNombrePer : null,
                    IdUsuarioActualiza = a.IdUsuarioActualiza,
                    NombreUsuarioActualiza = a.IdUsuarioActualizaNavigation != null ? a.IdUsuarioActualizaNavigation.IdPersonalNavigation!.ApellidosNombrePer : null
                })
                .ToListAsync();

            return Ok(atenciones);
        }

        // GET: api/atenciones/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AtencionDto>> GetAtencion(int id)
        {
            var atencion = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Include(a => a.IdUsuarioActualizaNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .FirstOrDefaultAsync(a => a.IdAtencion == id);

            if (atencion == null)
            {
                return NotFound();
            }

            var dto = new AtencionDto
            {
                IdAtencion = atencion.IdAtencion,
                TipoDocumento = atencion.TipoDocumento,
                NumeroDocumento = atencion.NumeroDocumento,
                Nombres = atencion.Nombres,
                Apellidos = atencion.Apellidos,
                IdTipoTramite = atencion.IdTipoTramite,
                NombreTramite = atencion.IdTipoTramiteNavigation.NombreTramite,
                DescripcionTramite = atencion.IdTipoTramiteNavigation.Descripcion,
                Observacion = atencion.Observacion,
                ObservacionAtencion = atencion.ObservacionAtencion,
                EsPreferencial = atencion.EsPreferencial,
                IdTipoPreferencial = atencion.IdTipoPreferencial,
                NombreTipoPreferencial = atencion.IdTipoPreferencialNavigation?.NombreTipoPreferencial,
                IdEstadoAtencion = atencion.IdEstadoAtencion,
                NombreEstado = atencion.IdEstadoAtencionNavigation.NombreEstado,
                OrdenEstado = atencion.IdEstadoAtencionNavigation.Orden,
                FechaRegistro = atencion.FechaRegistro,
                FechaActualizacion = atencion.FechaActualizacion,
                IdUsuarioRegistro = atencion.IdUsuarioRegistro,
                NombreUsuarioRegistro = atencion.IdUsuarioRegistroNavigation?.IdPersonalNavigation?.ApellidosNombrePer,
                IdUsuarioActualiza = atencion.IdUsuarioActualiza,
                NombreUsuarioActualiza = atencion.IdUsuarioActualizaNavigation?.IdPersonalNavigation?.ApellidosNombrePer
            };

            return Ok(dto);
        }

        // POST: api/atenciones
        [HttpPost]
        public async Task<ActionResult<AtencionDto>> CreateAtencion(AtencionCreateDto dto)
        {
            // Obtener el ID del usuario autenticado
            var userIdClaim = User.FindFirst("UsuarioId");
            int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

            // Obtener el estado inicial "Pendiente" (orden = 1)
            var estadoPendiente = await _context.UtdEstadoAtencions
                .FirstOrDefaultAsync(e => e.Orden == 1 && e.Estado);

            if (estadoPendiente == null)
            {
                return BadRequest("No se encontró el estado 'Pendiente'.");
            }

            var atencion = new UtdAtencion
            {
                TipoDocumento = dto.TipoDocumento,
                NumeroDocumento = dto.NumeroDocumento,
                Nombres = dto.Nombres,
                Apellidos = dto.Apellidos,
                IdTipoTramite = dto.IdTipoTramite,
                Observacion = dto.Observacion,
                EsPreferencial = dto.EsPreferencial,
                IdTipoPreferencial = dto.EsPreferencial ? dto.IdTipoPreferencial : null,
                IdEstadoAtencion = estadoPendiente.IdEstadoAtencion,
                FechaRegistro = DateTime.Now,
                IdUsuarioRegistro = userId
            };

            _context.UtdAtencions.Add(atencion);
            await _context.SaveChangesAsync();

            // Cargar las navegaciones para devolver el DTO completo
            await _context.Entry(atencion).Reference(a => a.IdTipoTramiteNavigation).LoadAsync();
            await _context.Entry(atencion).Reference(a => a.IdEstadoAtencionNavigation).LoadAsync();
            if (atencion.IdTipoPreferencial.HasValue)
            {
                await _context.Entry(atencion).Reference(a => a.IdTipoPreferencialNavigation).LoadAsync();
            }

            var result = new AtencionDto
            {
                IdAtencion = atencion.IdAtencion,
                TipoDocumento = atencion.TipoDocumento,
                NumeroDocumento = atencion.NumeroDocumento,
                Nombres = atencion.Nombres,
                Apellidos = atencion.Apellidos,
                IdTipoTramite = atencion.IdTipoTramite,
                NombreTramite = atencion.IdTipoTramiteNavigation.NombreTramite,
                Observacion = atencion.Observacion,
                ObservacionAtencion = atencion.ObservacionAtencion,
                EsPreferencial = atencion.EsPreferencial,
                IdTipoPreferencial = atencion.IdTipoPreferencial,
                NombreTipoPreferencial = atencion.IdTipoPreferencialNavigation?.NombreTipoPreferencial,
                IdEstadoAtencion = atencion.IdEstadoAtencion,
                NombreEstado = atencion.IdEstadoAtencionNavigation.NombreEstado,
                OrdenEstado = atencion.IdEstadoAtencionNavigation.Orden,
                FechaRegistro = atencion.FechaRegistro,
                IdUsuarioRegistro = atencion.IdUsuarioRegistro
            };

            return CreatedAtAction(nameof(GetAtencion), new { id = atencion.IdAtencion }, result);
        }

        // PUT: api/atenciones/{id}/estado
        [HttpPut("{id}/estado")]
        public async Task<ActionResult> UpdateEstadoAtencion(int id, [FromBody] AtencionUpdateDto dto)
        {
            try
            {
                var atencion = await _context.UtdAtencions.FindAsync(id);

                if (atencion == null)
                {
                    return NotFound(new { message = $"Atención con ID {id} no encontrada." });
                }

                // Obtener el ID del usuario autenticado
                var userIdClaim = User.FindFirst("UsuarioId");
                int? userId = userIdClaim != null ? int.Parse(userIdClaim.Value) : null;

                atencion.IdEstadoAtencion = dto.IdEstadoAtencion;
                
                // Solo actualizar Observacion si viene en el DTO
                if (dto.Observacion != null)
                {
                    atencion.Observacion = dto.Observacion;
                }
                
                // Solo actualizar ObservacionAtencion si viene en el DTO (cuando se finaliza)
                if (dto.ObservacionAtencion != null)
                {
                    atencion.ObservacionAtencion = dto.ObservacionAtencion;
                }
                
                atencion.FechaActualizacion = DateTime.Now;
                atencion.IdUsuarioActualiza = userId;

                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al actualizar el estado de la atención.", error = ex.Message });
            }
        }

        // GET: api/atenciones/tipos-tramite
        [HttpGet("tipos-tramite")]
        public async Task<ActionResult<IEnumerable<TipoTramiteDto>>> GetTiposTramite()
        {
            var tiposTramite = await _context.UtdTipoTramites
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoTramite)
                .Select(t => new TipoTramiteDto
                {
                    IdTipoTramite = t.IdTipoTramite,
                    NombreTramite = t.NombreTramite,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

            return Ok(tiposTramite);
        }

        // GET: api/atenciones/tipos-preferencial
        [HttpGet("tipos-preferencial")]
        public async Task<ActionResult<IEnumerable<TipoPreferencialDto>>> GetTiposPreferencial()
        {
            var tiposPreferencial = await _context.UtdTipoPreferencials
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoPreferencial)
                .Select(t => new TipoPreferencialDto
                {
                    IdTipoPreferencial = t.IdTipoPreferencial,
                    NombreTipoPreferencial = t.NombreTipoPreferencial,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

            return Ok(tiposPreferencial);
        }

        // GET: api/atenciones/estados
        [HttpGet("estados")]
        public async Task<ActionResult<IEnumerable<EstadoAtencionDto>>> GetEstados()
        {
            var estados = await _context.UtdEstadoAtencions
                .Where(e => e.Estado)
                .OrderBy(e => e.Orden)
                .Select(e => new EstadoAtencionDto
                {
                    IdEstadoAtencion = e.IdEstadoAtencion,
                    NombreEstado = e.NombreEstado,
                    Orden = e.Orden,
                    Descripcion = e.Descripcion
                })
                .ToListAsync();

            return Ok(estados);
        }
    }
}
