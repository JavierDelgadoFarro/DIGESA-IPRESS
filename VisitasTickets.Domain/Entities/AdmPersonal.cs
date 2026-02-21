using System;
using System.Collections.Generic;

namespace VisitasTickets.Domain.Entities
{
    public class AdmPersonal
    {
        public int IdPersonal { get; set; }

        public int? IdArea { get; set; }

        public string? Iniciales { get; set; }

        public string? Paterno { get; set; }

        public string? Materno { get; set; }

        public string? Nombre { get; set; }

        public string? ApellidosNombrePer { get; set; }

        public int? IdProfesion { get; set; }

        public string? Dniper { get; set; }

        public DateTime? FechaNacimientoPer { get; set; }

        public string? EmailPer { get; set; }

        public string? CargoMinsa { get; set; }

        public int? IdCargo { get; set; }

        public int? IdEstado { get; set; }

        public string? DomicilioPer { get; set; }

        public string? DistritoPer { get; set; }

        public string? RefPer { get; set; }

        public string? CondicionLaboralPer { get; set; }

        public string? AnexoPer { get; set; }

        public DateTime? FechaIngresoPer { get; set; }

        public string? NumeroSeguroPer { get; set; }

        public string? TelefonoPer { get; set; }

        public string? PrestacionSaludPer { get; set; }

        public string? RegimenPensionPer { get; set; }

        public string? TipoSangrePer { get; set; }

        public string? AlergiaPer { get; set; }

        public string? ContactoEmergenciaPer { get; set; }

        public string? ObservaccionPer { get; set; }

        public DateTime? FechaRegistroPer { get; set; }

        public int? IdUsuarioper { get; set; }

        public int? IdSede { get; set; }

        public int? IdTemp { get; set; }

        public string? Area { get; set; }

        public string? Direccion { get; set; }

        public virtual ICollection<AdmUsuario> AdmUsuarios { get; set; } = new List<AdmUsuario>();

        public virtual AdmArea? IdAreaNavigation { get; set; }
    }
}


