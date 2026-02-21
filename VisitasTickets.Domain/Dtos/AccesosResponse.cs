namespace VisitasTickets.Domain.Dtos
{
    public class AccesosResponse 
    { 
        public int UsuarioId { get; set; } 
        public List<Modulo> Modulos { get; set; } 
    }

    public class Modulo 
    { 
        public int IdModulo { get; set; } 
        public string NombreModulo { get; set; } 
        public List<Menu> Menus { get; set; } 
    }

    public class Menu 
    { 
        public int IdMenu { get; set; } 
        public string NombreMenu { get; set; } 
        public List<SubMenu> SubMenus { get; set; } 
    }

    public class SubMenu 
    { 
        public int IdSubMenu { get; set; } 
        public string NombreSubMenu { get; set; } 
        public string Ruta { get; set; } 
    }
}
