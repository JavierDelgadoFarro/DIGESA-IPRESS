using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Domain.Globals;
using VisitasTickets.Infrastructure.Persistence;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene estadísticas generales para el dashboard del administrador
        /// </summary>
        /// <returns>Objeto con todas las estadísticas del dashboard</returns>
        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasDashboardDto>> GetEstadisticasDashboard()
        {
            try
            {
                // Total de usuarios en el sistema (filtrado por área)
                var totalUsuarios = await _context.AdmUsuarios
                    .Where(u => u.IdArea == AppConfig.DefaultAreaId)
                    .CountAsync();

                // Atenciones por estado (orden < 4 = activas)
                var atencionesPendientes = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "Pendiente")
                    .CountAsync();

                var atencionesEnVentanilla = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "En Ventanilla")
                    .CountAsync();

                var atencionesEnPausa = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "En Pausa")
                    .CountAsync();

                // Atenciones de hoy
                var inicioHoy = DateTime.Today;
                var finHoy = inicioHoy.AddDays(1);
                var atencionesHoy = await _context.UtdAtencions
                    .Where(a => a.FechaRegistro >= inicioHoy && a.FechaRegistro < finHoy)
                    .CountAsync();

                // Atenciones del mes actual
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1);
                var atencionesMes = await _context.UtdAtencions
                    .Where(a => a.FechaRegistro >= inicioMes && a.FechaRegistro < finMes)
                    .CountAsync();

                // Atenciones preferenciales pendientes
                var atencionesPreferenciales = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "Pendiente" && a.EsPreferencial == true)
                    .CountAsync();

                // Última atención registrada
                var ultimaAtencion = await _context.UtdAtencions
                    .Include(a => a.IdTipoTramiteNavigation)
                    .OrderByDescending(a => a.FechaRegistro)
                    .FirstOrDefaultAsync();

                // Calcular tiempo promedio de atención (en minutos) para TODAS las atenciones completadas
                var atencionesCompletadas = await _context.UtdAtencions
                    .Include(a => a.IdEstadoAtencionNavigation)
                    .Where(a => a.IdEstadoAtencionNavigation.NombreEstado == "Atendido")
                    .ToListAsync();

                decimal tiempoPromedioMinutos = 0;
                if (atencionesCompletadas.Any())
                {
                    // Obtener el estado "En Ventanilla"
                    var estadoVentanilla = await _context.UtdEstadoAtencions
                        .FirstOrDefaultAsync(e => e.NombreEstado == "En Ventanilla");

                    if (estadoVentanilla != null)
                    {
                        var tiemposTotales = new List<int>();
                        foreach (var atencion in atencionesCompletadas)
                        {
                            // Obtener historial de estados para esta atención
                            var historial = await _context.UtdHistorialAtencions
                                .Where(h => h.IdAtencion == atencion.IdAtencion)
                                .OrderBy(h => h.FechaCambio)
                                .ToListAsync();

                            // Calcular solo el tiempo EN VENTANILLA (cuando está siendo atendido)
                            int tiempoEnVentanillaSegundos = 0;
                            for (int i = 0; i < historial.Count - 1; i++)
                            {
                                var actual = historial[i];
                                var siguiente = historial[i + 1];
                                
                                // Si el estado actual es "En Ventanilla", el TiempoEnEstadoAnterior del siguiente
                                // registro indica cuánto tiempo estuvo EN VENTANILLA
                                if (actual.IdEstadoNuevo == estadoVentanilla.IdEstadoAtencion)
                                {
                                    tiempoEnVentanillaSegundos += siguiente.TiempoEnEstadoAnterior ?? 0;
                                }
                            }

                            if (tiempoEnVentanillaSegundos > 0)
                            {
                                tiemposTotales.Add(tiempoEnVentanillaSegundos);
                            }
                        }

                        if (tiemposTotales.Any())
                        {
                            var promedioSegundos = tiemposTotales.Average();
                            tiempoPromedioMinutos = Math.Round((decimal)(promedioSegundos / 60), 2);
                        }
                    }
                }

                var estadisticas = new EstadisticasDashboardDto
                {
                    TotalUsuarios = totalUsuarios,
                    AtencionesPendientes = atencionesPendientes,
                    AtencionesEnVentanilla = atencionesEnVentanilla,
                    AtencionesEnPausa = atencionesEnPausa,
                    AtencionesHoy = atencionesHoy,
                    AtencionesMes = atencionesMes,
                    AtencionesPreferenciales = atencionesPreferenciales,
                    UltimaAtencionFecha = ultimaAtencion?.FechaRegistro,
                    UltimaAtencionCiudadano = ultimaAtencion != null 
                        ? $"{ultimaAtencion.Apellidos} {ultimaAtencion.Nombres}" 
                        : null,
                    UltimaAtencionTramite = ultimaAtencion?.IdTipoTramiteNavigation?.NombreTramite,
                    TiempoPromedioAtencionMinutos = tiempoPromedioMinutos
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas del dashboard", error = ex.Message });
            }
        }
    }
}
