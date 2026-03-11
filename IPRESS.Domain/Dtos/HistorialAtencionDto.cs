using System;

namespace IPRESS.Domain.Dtos
{
    public class HistorialAtencionDto
    {
        public int IdHistorial { get; set; }
        public int IdAtencion { get; set; }
        public int? IdEstadoAnterior { get; set; }
        public string? NombreEstadoAnterior { get; set; }
        public int IdEstadoNuevo { get; set; }
        public string NombreEstadoNuevo { get; set; } = string.Empty;
        public int OrdenEstadoNuevo { get; set; }
        public int? IdUsuario { get; set; }
        public string? NombreUsuario { get; set; }
        public DateTime FechaCambio { get; set; }
        public string? Observacion { get; set; }
        public int? TiempoEnEstadoAnterior { get; set; } // En SEGUNDOS (para mayor precisión)
        public int? MinutosEnEsteEstado { get; set; } // Calculado en SEGUNDOS: tiempo hasta el siguiente cambio
    }
}
