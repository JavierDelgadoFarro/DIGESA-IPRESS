using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmDetalleSubMenu
    {
        public int IdDsubmenu { get; set; }

        public int? IdSmenu { get; set; }

        public int? IdUsuario { get; set; }

        public int? Flag { get; set; }

        public virtual AdmUsuario IdUsuarioNavigation { get; set; } = null!;
        public virtual AdmSubMenu? IdSmenuNavigation { get; set; }
    }
}

