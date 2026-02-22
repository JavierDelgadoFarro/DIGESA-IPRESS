using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class UtdEstadoAtencion
    {
        public int IdEstadoAtencion { get; set; }

        public string NombreEstado { get; set; } = null!;

        public int Orden { get; set; }

        public string? Descripcion { get; set; }

        public bool Estado { get; set; }

        public DateTime FechaCreacion { get; set; }

        public virtual ICollection<UtdAtencion> UtdAtencions { get; set; } = new List<UtdAtencion>();
    }
}
