namespace VisitasTickets.Domain.Dtos
{
    public class TipoTrabajoDto
    {
        public int IdTipoTrabajo { get; set; }
        public string NombreTipoTrabajo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Estado { get; set; } = true;
    }
}
