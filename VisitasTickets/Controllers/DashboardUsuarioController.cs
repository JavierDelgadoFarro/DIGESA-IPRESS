using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using VisitasTickets.Domain.Dtos;
using VisitasTickets.Infrastructure.Persistence;

namespace VisitasTickets.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DashboardUsuarioController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DashboardUsuarioController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene estadísticas personales del usuario autenticado
        /// </summary>
        /// <returns>Objeto con las estadísticas del usuario</returns>
        [HttpGet("estadisticas")]
        public async Task<ActionResult<EstadisticasUsuarioDto>> GetEstadisticasUsuario()
        {
            try
            {
                // Obtener el ID del usuario autenticado
                var userIdClaim = User.FindFirst("UsuarioId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { mensaje = "No se pudo identificar el usuario" });
                }

                var inicioHoy = DateTime.Today;
                var finHoy = inicioHoy.AddDays(1);
                var inicioMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                var finMes = inicioMes.AddMonths(1);

                // Obtener IDs de estados
                var estados = await _context.UtdEstadoAtencions.ToListAsync();
                var estadoAtendido = estados.FirstOrDefault(e => e.NombreEstado == "Atendido");
                var estadoPausa = estados.FirstOrDefault(e => e.NombreEstado == "En Pausa");

                if (estadoAtendido == null)
                {
                    return StatusCode(500, new { mensaje = "Estado 'Atendido' no encontrado" });
                }

                // Obtener historial completo del usuario
                var historialCompleto = await _context.UtdHistorialAtencions
                    .Where(h => h.IdUsuario == userId)
                    .ToListAsync();

                // Filtrar en memoria los IDs donde cambió a "Atendido" HOY
                var idsAtendidasHoy = historialCompleto
                    .Where(h => h.IdEstadoNuevo == estadoAtendido.IdEstadoAtencion
                        && h.FechaCambio >= inicioHoy
                        && h.FechaCambio < finHoy)
                    .Select(h => h.IdAtencion)
                    .Distinct()
                    .ToList();

                // Filtrar en memoria los IDs donde cambió a "Atendido" ESTE MES
                var idsAtendidasMes = historialCompleto
                    .Where(h => h.IdEstadoNuevo == estadoAtendido.IdEstadoAtencion
                        && h.FechaCambio >= inicioMes
                        && h.FechaCambio < finMes)
                    .Select(h => h.IdAtencion)
                    .Distinct()
                    .ToList();

                // Filtrar en memoria TODAS las atenciones donde cambió a "Atendido"
                var idsAtendidas = historialCompleto
                    .Where(h => h.IdEstadoNuevo == estadoAtendido.IdEstadoAtencion)
                    .Select(h => h.IdAtencion)
                    .Distinct()
                    .ToList();

                // Contar atenciones hoy
                var atencionesHoy = idsAtendidasHoy.Count;

                // Contar atenciones mes
                var atencionesMes = idsAtendidasMes.Count;

                // Contar atenciones pausadas
                var atencionesPausadas = 0;
                if (estadoPausa != null)
                {
                    var idsPausadasPorUsuario = historialCompleto
                        .Where(h => h.IdEstadoNuevo == estadoPausa.IdEstadoAtencion)
                        .Select(h => h.IdAtencion)
                        .Distinct()
                        .ToList();

                    // Obtener solo las atenciones que necesitamos verificar
                    var todasLasAtenciones = await _context.UtdAtencions.ToListAsync();
                    atencionesPausadas = todasLasAtenciones
                        .Count(a => idsPausadasPorUsuario.Contains(a.IdAtencion) 
                            && a.IdEstadoAtencion == estadoPausa.IdEstadoAtencion);
                }

                // Total
                var totalAtenciones = idsAtendidas.Count;

                // Calcular tiempo promedio (TODAS las atenciones atendidas, no solo hoy)
                string tiempoPromedioFormateado = "0min";
                if (idsAtendidas.Any())
                {
                    // Obtener el estado "En Ventanilla"
                    var estadoVentanilla = estados.FirstOrDefault(e => e.NombreEstado == "En Ventanilla");
                    
                    if (estadoVentanilla == null)
                    {
                        tiempoPromedioFormateado = "0min";
                    }
                    else
                    {
                        var tiemposTotales = new List<int>();
                        foreach (var idAtencion in idsAtendidas)
                        {
                            var historialAtencion = historialCompleto
                                .Where(h => h.IdAtencion == idAtencion)
                                .OrderBy(h => h.FechaCambio)
                                .ToList();

                            // Calcular solo el tiempo EN VENTANILLA
                            int tiempoEnVentanillaSegundos = 0;
                            for (int i = 0; i < historialAtencion.Count - 1; i++)
                            {
                                var actual = historialAtencion[i];
                                var siguiente = historialAtencion[i + 1];
                                
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
                            var promedioSegundos = (int)tiemposTotales.Average();
                            int horas = promedioSegundos / 3600;
                            int minutos = (promedioSegundos % 3600) / 60;
                            int segundos = promedioSegundos % 60;

                            // Formatear según el tiempo
                            if (horas > 0)
                            {
                                tiempoPromedioFormateado = $"{horas}h {minutos}min";
                            }
                            else if (minutos > 0)
                            {
                                tiempoPromedioFormateado = $"{minutos}min {segundos}s";
                            }
                            else
                            {
                                tiempoPromedioFormateado = $"{segundos}s";
                            }
                        }
                    }
                }

                var estadisticas = new EstadisticasUsuarioDto
                {
                    AtencionesHoy = atencionesHoy,
                    AtencionesMes = atencionesMes,
                    AtencionesPausadas = atencionesPausadas,
                    TiempoPromedioAtencion = tiempoPromedioFormateado,
                    TotalAtenciones = totalAtenciones
                };

                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener estadísticas del usuario", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene las últimas atenciones completadas por el usuario hoy
        /// </summary>
        /// <returns>Lista de atenciones</returns>
        [HttpGet("mis-atenciones/hoy")]
        public async Task<ActionResult<List<AtencionDto>>> GetMisAtencionesHoy()
        {
            try
            {
                // Obtener el ID del usuario autenticado
                var userIdClaim = User.FindFirst("UsuarioId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return BadRequest(new { mensaje = "No se pudo identificar el usuario" });
                }

                var inicioHoy = DateTime.Today;
                var finHoy = inicioHoy.AddDays(1);

                // Obtener estados
                var estados = await _context.UtdEstadoAtencions.ToListAsync();
                var estadoAtendido = estados.FirstOrDefault(e => e.NombreEstado == "Atendido");
                
                if (estadoAtendido == null)
                {
                    return StatusCode(500, new { mensaje = "Estado 'Atendido' no encontrado" });
                }

                // Obtener historial del usuario
                var historialUsuario = await _context.UtdHistorialAtencions
                    .Where(h => h.IdUsuario == userId)
                    .ToListAsync();

                // Filtrar en memoria los IDs donde cambió a "Atendido" hoy
                var idsAtendidasHoy = historialUsuario
                    .Where(h => h.IdEstadoNuevo == estadoAtendido.IdEstadoAtencion
                        && h.FechaCambio >= inicioHoy
                        && h.FechaCambio < finHoy)
                    .Select(h => h.IdAtencion)
                    .Distinct()
                    .ToList();

                if (!idsAtendidasHoy.Any())
                {
                    return Ok(new List<AtencionDto>());
                }

                // Traer atenciones y sus relaciones
                var todasAtenciones = await _context.UtdAtencions.ToListAsync();
                var tiposTramite = await _context.UtdTipoTramites.ToListAsync();
                
                var atenciones = todasAtenciones
                    .Where(a => idsAtendidasHoy.Contains(a.IdAtencion))
                    .OrderByDescending(a => a.FechaRegistro)
                    .Take(10)
                    .Select(a => new AtencionDto
                    {
                        IdAtencion = a.IdAtencion,
                        TipoDocumento = a.TipoDocumento,
                        NumeroDocumento = a.NumeroDocumento,
                        Nombres = a.Nombres,
                        Apellidos = a.Apellidos,
                        IdTipoTramite = a.IdTipoTramite,
                        NombreTramite = tiposTramite.FirstOrDefault(t => t.IdTipoTramite == a.IdTipoTramite)?.NombreTramite ?? "",
                        EsPreferencial = a.EsPreferencial,
                        IdEstadoAtencion = a.IdEstadoAtencion,
                        NombreEstado = estados.FirstOrDefault(e => e.IdEstadoAtencion == a.IdEstadoAtencion)?.NombreEstado ?? "",
                        FechaRegistro = a.FechaRegistro,
                        FechaActualizacion = a.FechaActualizacion
                    })
                    .ToList();

                return Ok(atenciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error al obtener atenciones del usuario", error = ex.Message });
            }
        }
    }
}
