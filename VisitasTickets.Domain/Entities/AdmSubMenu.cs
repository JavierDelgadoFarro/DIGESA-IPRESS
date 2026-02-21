using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmSubMenu
    {
        public int IdSmenu { get; set; }

        public int? IdMenu { get; set; }

        public string? DescripcionSme { get; set; }

        public string? RutaWebSme { get; set; }

        public int? OrdenSme { get; set; }

        public int? IdEstado { get; set; }

        public virtual ICollection<AdmDetalleSubMenu> AdmDetalleSubMenus { get; set; } = new List<AdmDetalleSubMenu>();

        public virtual AdmMenu? IdMenuNavigation { get; set; }
    }
}

