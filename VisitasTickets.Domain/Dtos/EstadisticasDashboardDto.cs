using System;

namespace VisitasTickets.Domain.Dtos
{
    public class EstadisticasDashboardDto
    {
        public int TotalUsuarios { get; set; }
        public int AtencionesPendientes { get; set; }
        public int AtencionesEnVentanilla { get; set; }
        public int AtencionesEnPausa { get; set; }
        public int AtencionesHoy { get; set; }
        public int AtencionesMes { get; set; }
        public int AtencionesPreferenciales { get; set; }
        public DateTime? UltimaAtencionFecha { get; set; }
        public string? UltimaAtencionCiudadano { get; set; }
        public string? UltimaAtencionTramite { get; set; }
        public decimal TiempoPromedioAtencionMinutos { get; set; }
    }
}
