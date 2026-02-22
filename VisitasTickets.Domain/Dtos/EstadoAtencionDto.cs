namespace VisitasTickets.Domain.Dtos
{
    public class EstadoAtencionDto
    {
        public int IdEstadoAtencion { get; set; }
        public string NombreEstado { get; set; } = string.Empty;
        public int Orden { get; set; }
        public string? Descripcion { get; set; }
    }
}
