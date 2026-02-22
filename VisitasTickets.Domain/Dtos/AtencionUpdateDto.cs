namespace VisitasTickets.Domain.Dtos
{
    public class AtencionUpdateDto
    {
        public int IdAtencion { get; set; }
        public int IdEstadoAtencion { get; set; }
        public string? Observacion { get; set; }
        public string? ObservacionAtencion { get; set; }
    }
}
