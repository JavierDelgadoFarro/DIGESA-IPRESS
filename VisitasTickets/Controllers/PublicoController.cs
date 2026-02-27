using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Entities;
using VisitasTickets.Infrastructure.Persistence;
using VisitasTickets.API.Hubs;

namespace VisitasTickets.API.Controllers
{
    [Route("api/publico")]
    [ApiController]
    public class PublicoController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SignalRNotificationService _notificationService;

        public PublicoController(AppDbContext context, SignalRNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Registra una nueva atención desde el formulario público (QR)
        /// </summary>
        [HttpPost("registro")]
        public async Task<ActionResult<RegistroPublicoResponseDto>> RegistrarAtencion(AtencionCreateDto dto)
        {
            try
            {
                // Obtener el estado inicial "Pendiente" (orden = 1)
                var estadoPendiente = await _context.UtdEstadoAtencions
                    .FirstOrDefaultAsync(e => e.Orden == 1 && e.Estado);

                if (estadoPendiente == null)
                {
                    return BadRequest(new { mensaje = "No se encontró el estado 'Pendiente'." });
                }

                // Crear la atención
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
                    IdUsuarioRegistro = null // Registro público, sin usuario
                };

                _context.UtdAtencions.Add(atencion);
                await _context.SaveChangesAsync();

                // Registrar el historial inicial
                var historialInicial = new UtdHistorialAtencion
                {
                    IdAtencion = atencion.IdAtencion,
                    IdEstadoAnterior = null,
                    IdEstadoNuevo = estadoPendiente.IdEstadoAtencion,
                    IdUsuario = null, // Registro público
                    FechaCambio = atencion.FechaRegistro,
                    TiempoEnEstadoAnterior = null,
                    Observacion = "Registro desde formulario público (QR)"
                };

                _context.UtdHistorialAtencions.Add(historialInicial);
                await _context.SaveChangesAsync();

                // Notificar a todos los clientes sobre la nueva atención
                await _notificationService.NotificarNuevaAtencion();
                await _notificationService.NotificarActualizacionDashboard();

                // Cargar las navegaciones
                await _context.Entry(atencion).Reference(a => a.IdTipoTramiteNavigation).LoadAsync();
                await _context.Entry(atencion).Reference(a => a.IdEstadoAtencionNavigation).LoadAsync();

                // Calcular el número de atención del día y cuántas hay antes
                var response = await ConsultarTurnoPorAtencion(atencion);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al registrar la atención", error = ex.Message });
            }
        }

        /// <summary>
        /// Consulta el turno de una atención por ID único
        /// </summary>
        [HttpGet("turno/id/{idAtencion:int}")]
        public async Task<ActionResult<TurnoConsultaDto>> ConsultarTurnoPorId(int idAtencion)
        {
            try
            {
                var atencion = await _context.UtdAtencions
                    .Include(a => a.IdTipoTramiteNavigation)
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdAtencion == idAtencion)
                    .FirstOrDefaultAsync();

                if (atencion == null)
                {
                    return NotFound(new { mensaje = "No se encontró la atención solicitada." });
                }

                var response = await ConsultarTurnoPorAtencion(atencion);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al consultar el turno", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la lista de tipos de trámite disponibles
        /// </summary>
        [HttpGet("tramites")]
        public async Task<ActionResult<IEnumerable<TipoTramiteDto>>> GetTramites()
        {
            try
            {
                var tramites = await _context.UtdTipoTramites
                    .Where(t => t.Estado)
                    .OrderBy(t => t.NombreTramite)
                    .Select(t => new TipoTramiteDto
                    {
                        IdTipoTramite = t.IdTipoTramite,
                        NombreTramite = t.NombreTramite,
                        Descripcion = t.Descripcion,
                        Estado = t.Estado
                    })
                    .ToListAsync();

                return Ok(tramites);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener trámites", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene la lista de tipos preferenciales disponibles
        /// </summary>
        [HttpGet("tipos-preferencial")]
        public async Task<ActionResult<IEnumerable<TipoPreferencialDto>>> GetTiposPreferencial()
        {
            try
            {
                var tipos = await _context.UtdTipoPreferencials
                    .Where(t => t.Estado)
                    .OrderBy(t => t.NombreTipoPreferencial)
                    .Select(t => new TipoPreferencialDto
                    {
                        IdTipoPreferencial = t.IdTipoPreferencial,
                        NombreTipoPreferencial = t.NombreTipoPreferencial,
                        Estado = t.Estado
                    })
                    .ToListAsync();

                return Ok(tipos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener tipos preferenciales", error = ex.Message });
            }
        }

        /// <summary>
        /// Buscar datos de una persona por su número de documento
        /// </summary>
        [HttpGet("buscar-por-documento/{documento}")]
        public async Task<ActionResult> BuscarPorDocumento(string documento)
        {
            try
            {
                // Buscar la atención más reciente de este documento
                var atencion = await _context.UtdAtencions
                    .Where(a => a.NumeroDocumento == documento)
                    .OrderByDescending(a => a.FechaRegistro)
                    .Select(a => new
                    {
                        TipoDocumento = a.TipoDocumento,
                        NumeroDocumento = a.NumeroDocumento,
                        Nombres = a.Nombres,
                        Apellidos = a.Apellidos
                    })
                    .FirstOrDefaultAsync();

                if (atencion == null)
                {
                    return NotFound(new { mensaje = "No se encontraron datos para este documento" });
                }

                return Ok(atencion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al buscar documento", error = ex.Message });
            }
        }

        /// <summary>
        /// Método privado para calcular la información del turno
        /// </summary>
        private async Task<RegistroPublicoResponseDto> ConsultarTurnoPorAtencion(UtdAtencion atencion)
        {
            var inicioHoy = DateTime.Today;
            var finHoy = inicioHoy.AddDays(1);

            // Calcular el número de atención del día (orden cronológico)
            var numeroAtencion = await _context.UtdAtencions
                .Where(a => a.FechaRegistro >= inicioHoy &&
                           a.FechaRegistro < finHoy &&
                           a.FechaRegistro <= atencion.FechaRegistro)
                .CountAsync();

            // Calcular cuántas atenciones están pendientes ANTES de esta
            // NO se limita por fecha - incluye pendientes de días anteriores
            // Aplicar reglas de prioridad:
            // - Los preferenciales tienen prioridad sobre los normales
            // - Si la atención es PREFERENCIAL: solo cuenta preferenciales antes
            // - Si la atención es NORMAL: cuenta TODOS los preferenciales + normales antes
            
            int atencionesPendientesAntes;
            
            if (atencion.EsPreferencial)
            {
                // Si es preferencial, solo cuenta preferenciales pendientes que se registraron antes
                atencionesPendientesAntes = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a =>
                        a.FechaRegistro < atencion.FechaRegistro &&
                        a.EsPreferencial == true &&
                        a.IdEstadoAtencionNavigation.Orden <= 2) // Pendiente o En Ventanilla
                    .CountAsync();
            }
            else
            {
                // Si es normal, cuenta:
                // 1. TODOS los preferenciales pendientes (sin importar cuándo se registraron)
                var preferenciales = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a =>
                        a.EsPreferencial == true &&
                        a.IdEstadoAtencionNavigation.Orden <= 2) // Pendiente o En Ventanilla
                    .CountAsync();
                
                // 2. Normales pendientes que se registraron antes
                var normales = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a =>
                        a.FechaRegistro < atencion.FechaRegistro &&
                        a.EsPreferencial == false &&
                        a.IdEstadoAtencionNavigation.Orden <= 2) // Pendiente o En Ventanilla
                    .CountAsync();
                
                atencionesPendientesAntes = preferenciales + normales;
            }

            var tramite = atencion.IdTipoTramiteNavigation ?? await _context.UtdTipoTramites
                .FirstOrDefaultAsync(t => t.IdTipoTramite == atencion.IdTipoTramite);

            var estado = atencion.IdEstadoAtencionNavigation ?? await _context.UtdEstadoAtencions
                .FirstOrDefaultAsync(e => e.IdEstadoAtencion == atencion.IdEstadoAtencion);

            string mensaje = estado?.Orden switch
            {
                1 => $"Hay {atencionesPendientesAntes} persona(s) antes que usted",
                2 => "¡Es su turno! Diríjase a la ventanilla",
                3 => "Su atención está en pausa",
                4 => "Su atención ha sido completada",
                _ => "Estado desconocido"
            };

            // El número de orden es: cuántos hay antes + 1
            // Si hay 0 antes → #1, si hay 1 antes → #2, etc.
            int numeroOrden = atencionesPendientesAntes + 1;

            return new RegistroPublicoResponseDto
            {
                IdAtencion = atencion.IdAtencion,
                NumeroAtencion = numeroAtencion,
                NumeroOrden = numeroOrden,
                TipoDocumento = atencion.TipoDocumento,
                NumeroDocumento = atencion.NumeroDocumento,
                NombreCompleto = $"{atencion.Nombres} {atencion.Apellidos}",
                NombreTramite = tramite?.NombreTramite ?? "Desconocido",
                EstadoActual = estado?.NombreEstado ?? "Desconocido",
                FechaRegistro = atencion.FechaRegistro,
                AtencionesPendientesAntes = atencionesPendientesAntes
            };
        }
    }
}
