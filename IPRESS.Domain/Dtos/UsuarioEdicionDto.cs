namespace IPRESS.Domain.Dtos
{
    public class UsuarioEdicionDto
    {
        public int IdUsuario { get; set; }
        public string NombreUsuarioUsu { get; set; } = string.Empty;
        public string? Paterno { get; set; }
        public string? Materno { get; set; }
        public string? Nombre { get; set; }
        public List<int> SubMenusSeleccionados { get; set; } = new();
    }

}
