namespace VisitasTickets.Domain.Dtos
{
    public class UsuarioEdicionDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuarioUsu { get; set; }
        public string? Paterno { get; set; }
        public string? Materno { get; set; }
        public string? Nombre { get; set; }
    }

}
