using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmArea
    {
        public int IdArea { get; set; }

        public int? IdDireccion { get; set; }

        public string? DescripcionAre { get; set; }

        public string? AbreviaturasAre { get; set; }

        public int? IdEstado { get; set; }

        public int? IdPersonal { get; set; }

        public string? JefaturasAre { get; set; }

        public virtual ICollection<AdmPersonal> AdmPersonals { get; set; } = new List<AdmPersonal>();
    }
}

