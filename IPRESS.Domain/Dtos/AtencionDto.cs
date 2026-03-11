using System;

namespace IPRESS.Domain.Dtos
{
    public class AtencionDto
    {
        public int IdAtencion { get; set; }
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto => $"{Apellidos} {Nombres}";
        public int IdTipoTramite { get; set; }
        public string NombreTramite { get; set; } = string.Empty;
        public string? DescripcionTramite { get; set; }
        public string? Observacion { get; set; }
        public string? ObservacionAtencion { get; set; }
        public bool EsPreferencial { get; set; }
        public int? IdTipoPreferencial { get; set; }
        public string? NombreTipoPreferencial { get; set; }
        public int IdEstadoAtencion { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public int OrdenEstado { get; set; }
        public int NumeroOrden { get; set; } // Número de orden dinámico en la cola
        public int? IdTipoTrabajo { get; set; }
        public string? NombreTipoTrabajo { get; set; }
        public int? IdDetalleActividad { get; set; }
        public string? NombreActividad { get; set; }
        public string? NumeroExpediente { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? FechaActualizacion { get; set; }
        public int? IdUsuarioRegistro { get; set; }
        public string? NombreUsuarioRegistro { get; set; }
        public int? IdUsuarioActualiza { get; set; }
        public string? NombreUsuarioActualiza { get; set; }
    }
}
