using System;

namespace VisitasTickets.Domain.Entities
{
    public class UtdHistorialAtencion
    {
        public int IdHistorial { get; set; }

        public int IdAtencion { get; set; }

        public int? IdEstadoAnterior { get; set; }

        public int IdEstadoNuevo { get; set; }

        public int? IdUsuario { get; set; }

        public DateTime FechaCambio { get; set; }

        public string? Observacion { get; set; }

        public int? TiempoEnEstadoAnterior { get; set; } // En minutos

        // Navegación
        public virtual UtdAtencion IdAtencionNavigation { get; set; } = null!;

        public virtual UtdEstadoAtencion? IdEstadoAnteriorNavigation { get; set; }

        public virtual UtdEstadoAtencion IdEstadoNuevoNavigation { get; set; } = null!;

        public virtual AdmUsuario? IdUsuarioNavigation { get; set; }
    }
}
