using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Entities;
using VisitasTickets.Infrastructure.Persistence;
using VisitasTickets.API.Hubs;
using OfficeOpenXml;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AtencionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly SignalRNotificationService _notificationService;

        public AtencionesController(AppDbContext context, SignalRNotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        // GET: api/atenciones - Obtener todas las atenciones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtenciones()
        {
            var atenciones = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdTipoTrabajoNavigation)
                .Include(a => a.IdDetalleActividadNavigation)
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
                    IdTipoTrabajo = a.IdTipoTrabajo,
                    NombreTipoTrabajo = a.IdTipoTrabajoNavigation != null ? a.IdTipoTrabajoNavigation.NombreTipoTrabajo : null,
                    IdDetalleActividad = a.IdDetalleActividad,
                    NombreActividad = a.IdDetalleActividadNavigation != null ? a.IdDetalleActividadNavigation.NombreActividad : null,
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
                .Include(a => a.IdTipoTrabajoNavigation)
                .Include(a => a.IdDetalleActividadNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Where(a => a.IdEstadoAtencionNavigation.Orden < 4) // Pendiente, En Ventanilla y En Pausa
                .OrderByDescending(a => a.EsPreferencial) // Preferenciales primero
                .ThenBy(a => a.FechaRegistro) // Luego por orden de registro
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
                    IdTipoTrabajo = a.IdTipoTrabajo,
                    NombreTipoTrabajo = a.IdTipoTrabajoNavigation != null ? a.IdTipoTrabajoNavigation.NombreTipoTrabajo : null,
                    IdDetalleActividad = a.IdDetalleActividad,
                    NombreActividad = a.IdDetalleActividadNavigation != null ? a.IdDetalleActividadNavigation.NombreActividad : null,
                    NumeroExpediente = a.NumeroExpediente,
                    FechaRegistro = a.FechaRegistro,
                    FechaActualizacion = a.FechaActualizacion,
                    IdUsuarioRegistro = a.IdUsuarioRegistro,
                    NombreUsuarioRegistro = a.IdUsuarioRegistroNavigation != null ? a.IdUsuarioRegistroNavigation.IdPersonalNavigation!.ApellidosNombrePer : null
                })
                .ToListAsync();

            // Calcular el número de orden dinámicamente
            int numeroOrden = 1;
            foreach (var atencion in atenciones)
            {
                atencion.NumeroOrden = numeroOrden++;
            }

            return Ok(atenciones);
        }

        // GET: api/atenciones/historial - Obtener atenciones atendidas y canceladas
        [HttpGet("historial")]
        public async Task<ActionResult<IEnumerable<AtencionDto>>> GetAtencionesHistorial()
        {
            var atenciones = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdTipoTrabajoNavigation)
                .Include(a => a.IdDetalleActividadNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Include(a => a.IdUsuarioActualizaNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "Atendido" || a.IdEstadoAtencionNavigation.NombreEstado == "Cancelado")
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
                    IdTipoTrabajo = a.IdTipoTrabajo,
                    NombreTipoTrabajo = a.IdTipoTrabajoNavigation != null ? a.IdTipoTrabajoNavigation.NombreTipoTrabajo : null,
                    IdDetalleActividad = a.IdDetalleActividad,
                    NombreActividad = a.IdDetalleActividadNavigation != null ? a.IdDetalleActividadNavigation.NombreActividad : null,
                    NumeroExpediente = a.NumeroExpediente,
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

        // GET: api/atenciones/historial/excel - Generar reporte Excel del historial
        [HttpGet("historial/excel")]
        public async Task<IActionResult> DescargarHistorialExcel([FromQuery] int? usuarioId)
        {
            // Configurar licencia EPPlus (NonCommercial)
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Consultar atenciones del historial
            var query = _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdTipoTrabajoNavigation)
                .Include(a => a.IdDetalleActividadNavigation)
                .Include(a => a.IdUsuarioRegistroNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Include(a => a.IdUsuarioActualizaNavigation)
                    .ThenInclude(u => u!.IdPersonalNavigation)
                .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "Atendido" || a.IdEstadoAtencionNavigation.NombreEstado == "Cancelado");

            // Filtrar por usuario si se proporciona
            if (usuarioId.HasValue)
            {
                query = query.Where(a => a.IdUsuarioActualiza == usuarioId.Value);
            }

            var atenciones = await query
                .OrderByDescending(a => a.FechaActualizacion)
                .ToListAsync();

            // Crear archivo Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Historial de Atenciones");

                // Encabezados
                worksheet.Cells[1, 1].Value = "FECHA";
                worksheet.Cells[1, 2].Value = "HORA";
                worksheet.Cells[1, 3].Value = "USUARIO QUE ATENDIÓ";
                worksheet.Cells[1, 4].Value = "TIPO DE TRÁMITE";
                worksheet.Cells[1, 5].Value = "NOMBRE COMPLETO";
                worksheet.Cells[1, 6].Value = "DNI";
                worksheet.Cells[1, 7].Value = "EXPEDIENTE";
                worksheet.Cells[1, 8].Value = "DETALLE DE ACTIVIDAD";
                worksheet.Cells[1, 9].Value = "OBSERVACIONES";
                worksheet.Cells[1, 10].Value = "TIPO DE TRABAJO";

                // Estilo de encabezados
                using (var range = worksheet.Cells[1, 1, 1, 10])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(79, 70, 229)); // Indigo
                    range.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                }

                // Datos
                int row = 2;
                foreach (var atencion in atenciones)
                {
                    worksheet.Cells[row, 1].Value = atencion.FechaActualizacion?.ToString("dd/MM/yyyy") ?? "";
                    worksheet.Cells[row, 2].Value = atencion.FechaActualizacion?.ToString("HH:mm:ss") ?? "";
                    worksheet.Cells[row, 3].Value = atencion.IdUsuarioActualizaNavigation?.IdPersonalNavigation?.ApellidosNombrePer ?? "";
                    worksheet.Cells[row, 4].Value = atencion.IdTipoTramiteNavigation?.NombreTramite ?? "";
                    worksheet.Cells[row, 5].Value = $"{atencion.Apellidos} {atencion.Nombres}";
                    worksheet.Cells[row, 6].Value = atencion.NumeroDocumento ?? "";
                    worksheet.Cells[row, 7].Value = atencion.NumeroExpediente ?? "";
                    worksheet.Cells[row, 8].Value = atencion.IdDetalleActividadNavigation?.NombreActividad ?? "";
                    worksheet.Cells[row, 9].Value = atencion.ObservacionAtencion ?? "";
                    worksheet.Cells[row, 10].Value = atencion.IdTipoTrabajoNavigation?.NombreTipoTrabajo ?? "";
                    row++;
                }

                // Ajustar ancho de columnas
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Convertir a bytes
                var excelBytes = package.GetAsByteArray();

                // Nombre del archivo
                var fileName = $"Historial_Atenciones_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";

                // Devolver archivo
                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        // GET: api/atenciones/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AtencionDto>> GetAtencion(int id)
        {
            var atencion = await _context.UtdAtencions
                .Include(a => a.IdTipoTramiteNavigation)
                .Include(a => a.IdTipoPreferencialNavigation)
                .Include(a => a.IdEstadoAtencionNavigation)
                .Include(a => a.IdTipoTrabajoNavigation)
                .Include(a => a.IdDetalleActividadNavigation)
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
                IdTipoTrabajo = atencion.IdTipoTrabajo,
                NombreTipoTrabajo = atencion.IdTipoTrabajoNavigation?.NombreTipoTrabajo,
                IdDetalleActividad = atencion.IdDetalleActividad,
                NombreActividad = atencion.IdDetalleActividadNavigation?.NombreActividad,
                NumeroExpediente = atencion.NumeroExpediente,
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

            // Registrar el historial inicial
            var historialInicial = new UtdHistorialAtencion
            {
                IdAtencion = atencion.IdAtencion,
                IdEstadoAnterior = null, // No hay estado anterior
                IdEstadoNuevo = estadoPendiente.IdEstadoAtencion,
                IdUsuario = userId,
                FechaCambio = atencion.FechaRegistro,
                TiempoEnEstadoAnterior = null,
                Observacion = "Registro inicial de la atención"
            };
            
            _context.UtdHistorialAtencions.Add(historialInicial);
            await _context.SaveChangesAsync();

            // Notificar a todos los clientes sobre la nueva atención
            await _notificationService.NotificarNuevaAtencion();
            await _notificationService.NotificarActualizacionDashboard();

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

                // Guardar el estado anterior para el historial
                int? estadoAnterior = atencion.IdEstadoAtencion;
                DateTime fechaCambio = DateTime.Now;

                // Calcular tiempo en el estado anterior (en SEGUNDOS para mayor precisión)
                int? tiempoEnEstadoAnterior = null;
                
                // Buscar el último registro de historial para calcular el tiempo
                var ultimoHistorial = await _context.UtdHistorialAtencions
                    .Where(h => h.IdAtencion == id)
                    .OrderByDescending(h => h.FechaCambio)
                    .FirstOrDefaultAsync();
                
                if (ultimoHistorial != null)
                {
                    tiempoEnEstadoAnterior = (int)(fechaCambio - ultimoHistorial.FechaCambio).TotalSeconds;
                }
                else
                {
                    // Si no hay historial, calcular desde la fecha de registro
                    tiempoEnEstadoAnterior = (int)(fechaCambio - atencion.FechaRegistro).TotalSeconds;
                }

                // Actualizar la atención
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

                // Actualizar IdTipoTrabajo si viene en el DTO
                if (dto.IdTipoTrabajo.HasValue)
                {
                    atencion.IdTipoTrabajo = dto.IdTipoTrabajo;
                }

                // Actualizar IdDetalleActividad si viene en el DTO
                if (dto.IdDetalleActividad.HasValue)
                {
                    atencion.IdDetalleActividad = dto.IdDetalleActividad;
                }

                // Actualizar NumeroExpediente si viene en el DTO
                if (dto.NumeroExpediente != null)
                {
                    atencion.NumeroExpediente = dto.NumeroExpediente;
                }
                
                atencion.FechaActualizacion = fechaCambio;
                atencion.IdUsuarioActualiza = userId;

                // Registrar en el historial
                var historial = new UtdHistorialAtencion
                {
                    IdAtencion = id,
                    IdEstadoAnterior = estadoAnterior,
                    IdEstadoNuevo = dto.IdEstadoAtencion,
                    IdUsuario = userId,
                    FechaCambio = fechaCambio,
                    TiempoEnEstadoAnterior = tiempoEnEstadoAnterior,
                    Observacion = dto.ObservacionAtencion // Guardar la observación del cambio
                };

                _context.UtdHistorialAtencions.Add(historial);

                await _context.SaveChangesAsync();

                // Notificar a todos los clientes sobre el cambio de estado
                await _notificationService.NotificarCambioEstadoAtencion(id);
                await _notificationService.NotificarActualizacionDashboard();

                // Si el estado cambia a "En Ventanilla" (orden = 2), notificar al visitante
                var nuevoEstado = await _context.UtdEstadoAtencions.FindAsync(dto.IdEstadoAtencion);
                
                if (nuevoEstado != null && nuevoEstado.Orden == 2) // "En Ventanilla"
                {
                    // Calcular el número de atención del día
                    var inicioHoy = DateTime.Today;
                    var finHoy = inicioHoy.AddDays(1);
                    var numeroAtencion = await _context.UtdAtencions
                        .Where(a => a.FechaRegistro >= inicioHoy &&
                                   a.FechaRegistro < finHoy &&
                                   a.FechaRegistro <= atencion.FechaRegistro)
                        .CountAsync();
                    
                    await _notificationService.NotificarTurnoVisitante(
                        atencion.NumeroDocumento,
                        "¡Es su turno! Por favor diríjase a la ventanilla de atención",
                        numeroAtencion
                    );
                }

                // Notificar también actualización general de turnos para todos los visitantes
                await _notificationService.NotificarActualizacionTurnos();

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

        // GET: api/atenciones/tipos-trabajo
        [HttpGet("tipos-trabajo")]
        public async Task<ActionResult<IEnumerable<TipoTrabajoDto>>> GetTiposTrabajo()
        {
            var tiposTrabajo = await _context.UtdTipoTrabajos
                .Where(t => t.Estado)
                .OrderBy(t => t.IdTipoTrabajo)
                .Select(t => new TipoTrabajoDto
                {
                    IdTipoTrabajo = t.IdTipoTrabajo,
                    NombreTipoTrabajo = t.NombreTipoTrabajo,
                    Descripcion = t.Descripcion
                })
                .ToListAsync();

            return Ok(tiposTrabajo);
        }

        // GET: api/atenciones/detalles-actividad
        [HttpGet("detalles-actividad")]
        public async Task<ActionResult<IEnumerable<DetalleActividadDto>>> GetDetallesActividad()
        {
            var detallesActividad = await _context.UtdDetalleActividads
                .Where(d => d.Estado)
                .OrderBy(d => d.IdDetalleActividad)
                .Select(d => new DetalleActividadDto
                {
                    IdDetalleActividad = d.IdDetalleActividad,
                    NombreActividad = d.NombreActividad,
                    Descripcion = d.Descripcion
                })
                .ToListAsync();

            return Ok(detallesActividad);
        }

        // GET: api/atenciones/{id}/historial
        [HttpGet("{id}/historial")]
        public async Task<ActionResult<IEnumerable<HistorialAtencionDto>>> GetHistorialAtencion(int id)
        {
            // Verificar si la atención existe
            var atencionExiste = await _context.UtdAtencions.AnyAsync(a => a.IdAtencion == id);
            
            if (!atencionExiste)
            {
                return NotFound(new { message = $"Atención con ID {id} no encontrada." });
            }

            var historial = await _context.UtdHistorialAtencions
                .Where(h => h.IdAtencion == id)
                .Include(h => h.IdUsuarioNavigation!)
                    .ThenInclude(u => u.IdPersonalNavigation!)
                .Include(h => h.IdEstadoAnteriorNavigation)
                .Include(h => h.IdEstadoNuevoNavigation)
                .OrderBy(h => h.FechaCambio)
                .Select(h => new HistorialAtencionDto
                {
                    IdHistorial = h.IdHistorial,
                    IdAtencion = h.IdAtencion,
                    IdEstadoAnterior = h.IdEstadoAnterior,
                    NombreEstadoAnterior = h.IdEstadoAnteriorNavigation != null 
                        ? h.IdEstadoAnteriorNavigation.NombreEstado 
                        : null,
                    IdEstadoNuevo = h.IdEstadoNuevo,
                    NombreEstadoNuevo = h.IdEstadoNuevoNavigation.NombreEstado,
                    OrdenEstadoNuevo = h.IdEstadoNuevoNavigation.Orden,
                    IdUsuario = h.IdUsuario,
                    NombreUsuario = h.IdUsuarioNavigation != null && h.IdUsuarioNavigation.IdPersonalNavigation != null
                        ? h.IdUsuarioNavigation.IdPersonalNavigation.Nombre + " " + 
                          h.IdUsuarioNavigation.IdPersonalNavigation.Paterno + " " +
                          h.IdUsuarioNavigation.IdPersonalNavigation.Materno
                        : null,
                    FechaCambio = h.FechaCambio,
                    Observacion = h.Observacion,
                    TiempoEnEstadoAnterior = h.TiempoEnEstadoAnterior
                })
                .ToListAsync();

            // Asignar el tiempo en cada estado usando los datos guardados en BD
            for (int i = 0; i < historial.Count; i++)
            {
                if (i < historial.Count - 1)
                {
                    // No es el último registro, usar el TiempoEnEstadoAnterior del siguiente registro
                    // (ese campo guarda cuánto tiempo estuvo en el estado actual antes de cambiar, en SEGUNDOS)
                    historial[i].MinutosEnEsteEstado = historial[i + 1].TiempoEnEstadoAnterior;
                }
                else
                {
                    // Es el último registro (estado actual)
                    // Solo calcular tiempo si NO es un estado final (Atendido tiene Orden >= 4)
                    if (historial[i].OrdenEstadoNuevo < 4)
                    {
                        // Aún está en proceso (Pendiente, En Ventanilla o En Pausa)
                        var tiempoEnSegundos = (int)(DateTime.Now - historial[i].FechaCambio).TotalSeconds;
                        historial[i].MinutosEnEsteEstado = tiempoEnSegundos;
                    }
                    else
                    {
                        // Es un estado final (Atendido), no mostrar tiempo
                        historial[i].MinutosEnEsteEstado = null;
                    }
                }
            }

            return Ok(historial);
        }

        // GET: api/atenciones/{id}/tiempo-activo
        // Calcula el tiempo activo en ventanilla (excluyendo pausas)
        [HttpGet("{id}/tiempo-activo")]
        public async Task<ActionResult<object>> GetTiempoActivoVentanilla(int id)
        {
            // Verificar si la atención existe
            var atencion = await _context.UtdAtencions
                .Include(a => a.IdEstadoAtencionNavigation)
                .FirstOrDefaultAsync(a => a.IdAtencion == id);
            
            if (atencion == null)
            {
                return NotFound(new { message = $"Atención con ID {id} no encontrada." });
            }

            // Obtener todo el historial de la atención
            var historial = await _context.UtdHistorialAtencions
                .Include(h => h.IdEstadoNuevoNavigation)
                .Where(h => h.IdAtencion == id)
                .OrderBy(h => h.FechaCambio)
                .ToListAsync();

            if (historial.Count == 0)
            {
                return Ok(new { 
                    tiempoActivoMinutos = 0,
                    tiempoActivoSegundos = 0,
                    estadoActual = atencion.IdEstadoAtencionNavigation?.NombreEstado,
                    estadoActivo = false
                });
            }

            int tiempoTotalMinutos = 0;
            DateTime? inicioVentanilla = null;

            // Recorrer el historial para calcular tiempo en "En Ventanilla"
            for (int i = 0; i < historial.Count; i++)
            {
                var estadoNombre = historial[i].IdEstadoNuevoNavigation?.NombreEstado;

                if (estadoNombre == "En Ventanilla")
                {
                    // Marcar inicio del periodo en ventanilla
                    inicioVentanilla = historial[i].FechaCambio;
                }
                else if (inicioVentanilla.HasValue && (estadoNombre == "En Pausa" || estadoNombre == "Atendido"))
                {
                    // Fin del periodo en ventanilla, calcular tiempo
                    var tiempoEnMinutos = (int)(historial[i].FechaCambio - inicioVentanilla.Value).TotalMinutes;
                    tiempoTotalMinutos += tiempoEnMinutos;
                    inicioVentanilla = null;
                }
            }

            // Si actualmente está en ventanilla, sumar el tiempo actual
            bool estadoActivo = false;
            if (inicioVentanilla.HasValue && atencion.IdEstadoAtencionNavigation?.NombreEstado == "En Ventanilla")
            {
                var tiempoActual = (int)(DateTime.Now - inicioVentanilla.Value).TotalMinutes;
                tiempoTotalMinutos += tiempoActual;
                estadoActivo = true;
            }

            // Calcular en segundos con precisión
            int tiempoTotalSegundos = 0;
            
            // Sumar todos los periodos cerrados en segundos
            for (int i = 0; i < historial.Count; i++)
            {
                var estadoNombre = historial[i].IdEstadoNuevoNavigation?.NombreEstado;
                
                if (estadoNombre == "En Ventanilla" && i + 1 < historial.Count)
                {
                    var siguienteEstado = historial[i + 1].IdEstadoNuevoNavigation?.NombreEstado;
                    if (siguienteEstado == "En Pausa" || siguienteEstado == "Atendido")
                    {
                        // Periodo cerrado, calcular en segundos
                        var segundos = (int)(historial[i + 1].FechaCambio - historial[i].FechaCambio).TotalSeconds;
                        tiempoTotalSegundos += segundos;
                    }
                }
            }
            
            // Si actualmente está en ventanilla, sumar tiempo en curso
            if (estadoActivo && inicioVentanilla.HasValue)
            {
                var segundosActuales = (int)(DateTime.Now - inicioVentanilla.Value).TotalSeconds;
                tiempoTotalSegundos += segundosActuales;
            }

            return Ok(new
            {
                tiempoActivoMinutos = tiempoTotalMinutos,
                tiempoActivoSegundos = tiempoTotalSegundos,
                estadoActual = atencion.IdEstadoAtencionNavigation?.NombreEstado,
                estadoActivo = estadoActivo,
                ordenEstado = atencion.IdEstadoAtencionNavigation?.Orden
            });
        }
    }
}
