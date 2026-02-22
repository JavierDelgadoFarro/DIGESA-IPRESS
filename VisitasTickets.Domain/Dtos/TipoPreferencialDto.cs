namespace VisitasTickets.Domain.Dtos
{
    public class TipoPreferencialDto
    {
        public int IdTipoPreferencial { get; set; }
        public string NombreTipoPreferencial { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
