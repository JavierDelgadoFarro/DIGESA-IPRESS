namespace VisitasTickets.Domain.Dtos
{
    public class RegistroPublicoResponseDto
    {
        public int IdAtencion { get; set; }
        public int NumeroAtencion { get; set; } // Número secuencial del día
        public int NumeroOrden { get; set; } // Número de orden dinámico en la cola actual
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreTramite { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = "Pendiente";
        public DateTime FechaRegistro { get; set; }
        public int AtencionesPendientesAntes { get; set; } // Cuántas personas hay antes
    }
}
