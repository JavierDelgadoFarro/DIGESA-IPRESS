namespace IPRESS.Domain.Dtos
{
    public class AccesosResponse 
    { 
        public int UsuarioId { get; set; } 
        public List<Modulo> Modulos { get; set; } = new();
    }

    public class Modulo 
    { 
        public int IdModulo { get; set; } 
        public string NombreModulo { get; set; } = "";
        public List<Menu> Menus { get; set; } = new();
    }

    public class Menu 
    { 
        public int IdMenu { get; set; } 
        public string NombreMenu { get; set; } = "";
        public List<SubMenu> SubMenus { get; set; } = new();
    }

    public class SubMenu 
    { 
        public int IdSubMenu { get; set; } 
        public string NombreSubMenu { get; set; } = "";
        public string Ruta { get; set; } = "";
        public string? Icono { get; set; }
        public List<string> Botones { get; set; } = new(); // CREAR, EDITAR, ELIMINAR, IMPORTAR, EXPORTAR
    }
}
