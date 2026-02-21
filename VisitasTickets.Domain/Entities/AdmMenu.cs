using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmMenu
    {
        public int IdMenu { get; set; }

        public int? IdModulo { get; set; }

        public string? DescripcionMen { get; set; }

        public int? OrdenMen { get; set; }

        public int? IdEstado { get; set; }

        public virtual ICollection<AdmSubMenu> AdmSubMenus { get; set; } = new List<AdmSubMenu>();

        public virtual AdmModulo? IdModuloNavigation { get; set; }
    }
}

