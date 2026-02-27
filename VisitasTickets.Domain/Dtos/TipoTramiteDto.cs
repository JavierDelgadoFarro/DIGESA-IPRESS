namespace VisitasTickets.Domain.Dtos
{
    public class TipoTramiteDto
    {
        public int IdTipoTramite { get; set; }
        public string NombreTramite { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
