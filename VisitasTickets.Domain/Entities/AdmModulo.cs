using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmModulo
    {
        public int IdModulo { get; set; }

        public string? Identificador { get; set; }

        public string? DescripcionMod { get; set; }

        public int? IdEstado { get; set; }

        public virtual ICollection<AdmMenu> AdmMenus { get; set; } = new List<AdmMenu>();
    }
}

