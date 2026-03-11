namespace IPRESS.Domain.Dtos
{
    public class TurnoConsultaDto
    {
        public int IdAtencion { get; set; }
        public int NumeroAtencion { get; set; }
        public int NumeroOrden { get; set; } // Número de orden dinámico en la cola actual
        public string TipoDocumento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string NombreTramite { get; set; } = string.Empty;
        public string EstadoActual { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public int AtencionesPendientesAntes { get; set; } // Cuántas personas faltan antes
        public bool EsSuTurno { get; set; } // Si está "En Ventanilla"
        public string? Mensaje { get; set; } // Mensaje personalizado según el estado
    }
}
