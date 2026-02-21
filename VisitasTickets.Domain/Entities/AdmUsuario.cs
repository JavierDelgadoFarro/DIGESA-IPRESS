using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisitasTickets.Domain.Entities
{
    public class AdmUsuario
    {
        public int IdUsuario { get; set; }

        public int? IdPersonal { get; set; }

        public string? NombreUsuarioUsu { get; set; }

        public string? ContrasenaUsu { get; set; }

        public int? IdSede { get; set; }

        public int? IdArea { get; set; }

        public int? IdEstado { get; set; }

        public virtual AdmPersonal? IdPersonalNavigation { get; set; }

        public virtual ICollection<AdmDetalleSubMenu> AdmDetalleSubMenus { get; set; } = new List<AdmDetalleSubMenu>();
         
    }
}

