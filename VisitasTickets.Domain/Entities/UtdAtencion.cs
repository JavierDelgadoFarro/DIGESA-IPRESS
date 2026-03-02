using System;

namespace VisitasTickets.Domain.Entities
{
    public class UtdAtencion
    {
        public int IdAtencion { get; set; }

        public string TipoDocumento { get; set; } = null!;

        public string NumeroDocumento { get; set; } = null!;

        public string Nombres { get; set; } = null!;

        public string Apellidos { get; set; } = null!;

        public int IdTipoTramite { get; set; }

        public string? Observacion { get; set; }

        public string? ObservacionAtencion { get; set; }

        public bool EsPreferencial { get; set; }

        public int? IdTipoPreferencial { get; set; }

        public int IdEstadoAtencion { get; set; }

    public int? IdTipoTrabajo { get; set; }

    public int? IdDetalleActividad { get; set; }

    public string? NumeroExpediente { get; set; }

    public DateTime FechaRegistro { get; set; }

    public DateTime? FechaActualizacion { get; set; }

        public int? IdUsuarioRegistro { get; set; }

        public int? IdUsuarioActualiza { get; set; }

        public virtual UtdTipoTramite IdTipoTramiteNavigation { get; set; } = null!;

        public virtual UtdTipoPreferencial? IdTipoPreferencialNavigation { get; set; }

        public virtual UtdEstadoAtencion IdEstadoAtencionNavigation { get; set; } = null!;

    public virtual UtdTipoTrabajo? IdTipoTrabajoNavigation { get; set; }

    public virtual UtdDetalleActividad? IdDetalleActividadNavigation { get; set; }

    public virtual AdmUsuario? IdUsuarioRegistroNavigation { get; set; }

    public virtual AdmUsuario? IdUsuarioActualizaNavigation { get; set; }
}
}
