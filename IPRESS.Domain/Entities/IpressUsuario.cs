namespace IPRESS.Domain.Entities
{
    public class IpressUsuario
    {
        public int IdUsuario { get; set; }
        public string NombreUsuario { get; set; } = "";
        public string Contrasena { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string? Email { get; set; }
        public bool Activo { get; set; } = true;
        public int? IdDiresa { get; set; }
        public int? IdRed { get; set; }
        public int? IdMicroRed { get; set; }
        public int? IdEstablecimiento { get; set; }
        public DateTime? FechaCreacion { get; set; }

        public virtual ICollection<IpressUsuarioRol> UsuarioRoles { get; set; } = new List<IpressUsuarioRol>();
    }

    public class IpressUsuarioRol
    {
        public int IdUsuario { get; set; }
        public int IdRol { get; set; }
        public virtual IpressUsuario? Usuario { get; set; }
        public virtual IpressRol? Rol { get; set; }
    }

    public class IpressRol
    {
        public int IdRol { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public virtual ICollection<IpressRolModulo> RolModulos { get; set; } = new List<IpressRolModulo>();
        public virtual ICollection<IpressRolBoton> RolBotones { get; set; } = new List<IpressRolBoton>();
        public virtual ICollection<IpressRolSubMenu> RolSubMenus { get; set; } = new List<IpressRolSubMenu>();
    }

    public class IpressRolModulo
    {
        public int IdRol { get; set; }
        public int IdModulo { get; set; }
        public virtual IpressRol? Rol { get; set; }
        public virtual IpressModulo? Modulo { get; set; }
    }

    public class IpressRolBoton
    {
        public int IdRol { get; set; }
        public int IdBoton { get; set; }
        public virtual IpressRol? Rol { get; set; }
        public virtual IpressBoton? Boton { get; set; }
    }

    public class IpressModulo
    {
        public int IdModulo { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string? Ruta { get; set; }
        public string? Descripcion { get; set; }
        public int Orden { get; set; }
    }

    public class IpressBoton
    {
        public int IdBoton { get; set; }
        public int IdModulo { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
    }

    public class IpressMenu
    {
        public int IdMenu { get; set; }
        public int IdModulo { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public int Orden { get; set; }
        public virtual IpressModulo? Modulo { get; set; }
        public virtual ICollection<IpressSubMenu> SubMenus { get; set; } = new List<IpressSubMenu>();
    }

    public class IpressSubMenu
    {
        public int IdSubMenu { get; set; }
        public int IdMenu { get; set; }
        public string Codigo { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Ruta { get; set; } = "";
        public string? Icono { get; set; }
        public int Orden { get; set; }
        public virtual IpressMenu? Menu { get; set; }
    }

    public class IpressRolSubMenu
    {
        public int IdRol { get; set; }
        public int IdSubMenu { get; set; }
        public virtual IpressRol? Rol { get; set; }
        public virtual IpressSubMenu? SubMenu { get; set; }
    }
}
