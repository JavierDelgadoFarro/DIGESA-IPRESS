namespace IPRESS.Domain.Dtos
{
    public class UsuarioDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuarioUsu { get; set; } = "";
        public string? ApellidosNombrePer { get; set; }
        public string EstadoTexto { get; set; } = "";
    }
}
