using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class UtdTipoTrabajo
    {
        public int IdTipoTrabajo { get; set; }

        public string NombreTipoTrabajo { get; set; } = null!;

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual ICollection<UtdAtencion> UtdAtencions { get; set; } = new List<UtdAtencion>();
    }
}
