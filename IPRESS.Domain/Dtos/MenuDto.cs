namespace IPRESS.Domain.Dtos
{
    public class MenuDto
    {
        public int IdMenu { get; set; }
        public string NombreMenu { get; set; } = string.Empty;
        public List<SubMenuDto> SubMenus { get; set; } = new();
    }

    public class SubMenuDto
    {
        public int IdSubMenu { get; set; }
        public string NombreSubMenu { get; set; } = string.Empty;
        public string? Ruta { get; set; }
        public bool Seleccionado { get; set; } = false;
    }
}
