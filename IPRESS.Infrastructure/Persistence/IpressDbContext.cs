using Microsoft.EntityFrameworkCore;
using IPRESS.Domain.Entities;
using IPRESS.Domain.Interfaces;

namespace IPRESS.Infrastructure.Persistence
{
    public class IpressDbContext : DbContext
    {
        private readonly IAuditUserIdProvider? _auditUserIdProvider;

        public IpressDbContext(DbContextOptions<IpressDbContext> options) : base(options) { }

        public IpressDbContext(DbContextOptions<IpressDbContext> options, IAuditUserIdProvider auditUserIdProvider) : base(options)
        {
            _auditUserIdProvider = auditUserIdProvider;
        }

        public DbSet<IpressUsuario> Usuarios { get; set; }
        public DbSet<IpressUsuarioRol> UsuarioRoles { get; set; }
        public DbSet<IpressRol> Roles { get; set; }
        public DbSet<IpressRolModulo> RolModulos { get; set; }
        public DbSet<IpressRolBoton> RolBotones { get; set; }
        public DbSet<IpressModulo> Modulos { get; set; }
        public DbSet<IpressMenu> Menus { get; set; }
        public DbSet<IpressSubMenu> SubMenus { get; set; }
        public DbSet<IpressRolSubMenu> RolSubMenus { get; set; }
        public DbSet<IpressBoton> Botones { get; set; }
        public DbSet<IpressDepartamento> Departamentos { get; set; }
        public DbSet<IpressProvincia> Provincias { get; set; }
        public DbSet<IpressDistrito> Distritos { get; set; }
        public DbSet<IpressDiresa> Diresas { get; set; }
        public DbSet<IpressRed> Redes { get; set; }
        public DbSet<IpressMicroRed> MicroRedes { get; set; }
        public DbSet<IpressCentroPoblado> CentrosPoblados { get; set; }
        public DbSet<IpressCentroPobladoAccesibilidad> CentroPobladoAccesibilidades { get; set; }
        public DbSet<IpressCentroPobladoCentroEducativo> CentroPobladoCentrosEducativos { get; set; }
        public DbSet<IpressCentroPobladoAutoridad> CentroPobladoAutoridades { get; set; }
        public DbSet<IpressEstablecimiento> Establecimientos { get; set; }
        public DbSet<IpressEstablecimientoCentroPoblado> EstablecimientoCentrosPoblados { get; set; }

        protected override void OnModelCreating(ModelBuilder mb)
        {
            mb.Entity<IpressUsuario>(e =>
            {
                e.ToTable("IPRESS_Usuario", t => t.HasTrigger("TR_IPRESS_Usuario_Audit"));
                e.HasKey(x => x.IdUsuario);
                e.Property(x => x.IdUsuario).HasColumnName("ID_USUARIO");
                e.Property(x => x.NombreUsuario).HasColumnName("NombreUsuario");
                e.Property(x => x.Contrasena).HasColumnName("Password");
                e.Property(x => x.NombreCompleto).HasColumnName("NombreCompleto");
                e.Property(x => x.Email).HasColumnName("Email");
                e.Property(x => x.Activo).HasColumnName("Activo");
                e.Property(x => x.IdDiresa).HasColumnName("ID_DIRESA");
                e.Property(x => x.IdRed).HasColumnName("ID_RED");
                e.Property(x => x.IdMicroRed).HasColumnName("ID_MICRORED");
                e.Property(x => x.IdEstablecimiento).HasColumnName("ID_ESTABLECIMIENTO");
                e.Property(x => x.FechaCreacion).HasColumnName("FechaCreacion");
                e.HasMany(u => u.UsuarioRoles).WithOne(ur => ur.Usuario).HasForeignKey(ur => ur.IdUsuario);
                e.HasOne<IpressDiresa>().WithMany().HasForeignKey(x => x.IdDiresa).IsRequired(false);
                e.HasOne<IpressRed>().WithMany().HasForeignKey(x => x.IdRed).IsRequired(false);
                e.HasOne<IpressMicroRed>().WithMany().HasForeignKey(x => x.IdMicroRed).IsRequired(false);
                e.HasOne<IpressEstablecimiento>().WithMany().HasForeignKey(x => x.IdEstablecimiento).IsRequired(false);
            });

            mb.Entity<IpressUsuarioRol>(e =>
            {
                e.ToTable("IPRESS_UsuarioRol");
                e.HasKey(x => new { x.IdUsuario, x.IdRol });
                e.Property(x => x.IdUsuario).HasColumnName("ID_USUARIO");
                e.Property(x => x.IdRol).HasColumnName("ID_ROL");
                e.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
            });

            mb.Entity<IpressRol>(e =>
            {
                e.ToTable("IPRESS_Rol");
                e.HasKey(x => x.IdRol);
                e.Property(x => x.IdRol).HasColumnName("ID_ROL");
                e.Property(x => x.Codigo).HasColumnName("Codigo");
                e.Property(x => x.Nombre).HasColumnName("Nombre");
                e.HasMany(x => x.RolBotones).WithOne(rb => rb.Rol).HasForeignKey(rb => rb.IdRol);
                e.HasMany(x => x.RolSubMenus).WithOne(rs => rs.Rol).HasForeignKey(rs => rs.IdRol);
            });

            mb.Entity<IpressRolModulo>(e =>
            {
                e.ToTable("IPRESS_RolModulo");
                e.HasKey(x => new { x.IdRol, x.IdModulo });
                e.Property(x => x.IdRol).HasColumnName("ID_ROL");
                e.Property(x => x.IdModulo).HasColumnName("ID_MODULO");
                e.HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
                e.HasOne(x => x.Modulo).WithMany().HasForeignKey(x => x.IdModulo);
            });

            mb.Entity<IpressRolBoton>(e =>
            {
                e.ToTable("IPRESS_RolBoton");
                e.HasKey(x => new { x.IdRol, x.IdBoton });
                e.Property(x => x.IdRol).HasColumnName("ID_ROL");
                e.Property(x => x.IdBoton).HasColumnName("ID_BOTON");
                e.HasOne(x => x.Boton).WithMany().HasForeignKey(x => x.IdBoton);
            });

            mb.Entity<IpressModulo>(e =>
            {
                e.ToTable("IPRESS_Modulo");
                e.HasKey(x => x.IdModulo);
                e.Property(x => x.IdModulo).HasColumnName("ID_MODULO");
            });
            mb.Entity<IpressMenu>(e =>
            {
                e.ToTable("IPRESS_Menu");
                e.HasKey(x => x.IdMenu);
                e.Property(x => x.IdMenu).HasColumnName("ID_MENU");
                e.Property(x => x.IdModulo).HasColumnName("ID_MODULO");
                e.HasOne(x => x.Modulo).WithMany().HasForeignKey(x => x.IdModulo);
            });
            mb.Entity<IpressSubMenu>(e =>
            {
                e.ToTable("IPRESS_SubMenu");
                e.HasKey(x => x.IdSubMenu);
                e.Property(x => x.IdSubMenu).HasColumnName("ID_SUBMENU");
                e.Property(x => x.IdMenu).HasColumnName("ID_MENU");
                e.HasOne(x => x.Menu).WithMany(x => x.SubMenus).HasForeignKey(x => x.IdMenu);
            });
            mb.Entity<IpressRolSubMenu>(e =>
            {
                e.ToTable("IPRESS_RolSubMenu");
                e.HasKey(x => new { x.IdRol, x.IdSubMenu });
                e.Property(x => x.IdRol).HasColumnName("ID_ROL");
                e.Property(x => x.IdSubMenu).HasColumnName("ID_SUBMENU");
                e.HasOne(x => x.SubMenu).WithMany().HasForeignKey(x => x.IdSubMenu);
            });
            mb.Entity<IpressBoton>(e =>
            {
                e.ToTable("IPRESS_Boton");
                e.HasKey(x => x.IdBoton);
                e.Property(x => x.IdBoton).HasColumnName("ID_BOTON");
                e.Property(x => x.IdModulo).HasColumnName("ID_MODULO");
                e.HasOne<IpressModulo>().WithMany().HasForeignKey(x => x.IdModulo);
            });

            mb.Entity<IpressDepartamento>(e =>
            {
                e.ToTable("IPRESS_Departamento");
                e.HasKey(x => x.Codigo);
                e.Property(x => x.Codigo).HasMaxLength(2).HasColumnName("Codigo");
                e.Property(x => x.Nombre).HasMaxLength(100).HasColumnName("Nombre");
            });
            mb.Entity<IpressProvincia>(e =>
            {
                e.ToTable("IPRESS_Provincia");
                e.HasKey(x => x.Codigo);
                e.Property(x => x.Codigo).HasMaxLength(4).HasColumnName("Codigo");
                e.Property(x => x.Nombre).HasMaxLength(100).HasColumnName("Nombre");
                e.Property(x => x.CodigoDepartamento).HasMaxLength(2).HasColumnName("CodigoDepartamento");
                e.HasOne(x => x.Departamento).WithMany(x => x.Provincias).HasForeignKey(x => x.CodigoDepartamento);
            });
            mb.Entity<IpressDistrito>(e =>
            {
                e.ToTable("IPRESS_Distrito");
                e.HasKey(x => x.Ubigeo);
                e.Property(x => x.Ubigeo).HasMaxLength(6).HasColumnName("Ubigeo");
                e.Property(x => x.Nombre).HasMaxLength(100).HasColumnName("Nombre");
                e.Property(x => x.CodigoProvincia).HasMaxLength(4).HasColumnName("CodigoProvincia");
                e.HasOne(x => x.Provincia).WithMany(x => x.Distritos).HasForeignKey(x => x.CodigoProvincia);
            });
            mb.Entity<IpressDiresa>(e =>
            {
                e.ToTable("IPRESS_Diresa", t => t.HasTrigger("TR_IPRESS_Diresa_Audit"));
                e.HasKey(x => x.IdDiresa);
                e.Property(x => x.IdDiresa).HasColumnName("ID_DIRESA").ValueGeneratedOnAdd();
                e.Property(x => x.Codigo).HasColumnName("Codigo");
                e.Property(x => x.Nombre).HasColumnName("Nombre");
                e.Property(x => x.Ubigeo).HasMaxLength(6).HasColumnName("Ubigeo");
                e.HasOne(x => x.Distrito).WithMany().HasForeignKey(x => x.Ubigeo).IsRequired(false);
            });
            mb.Entity<IpressRed>(e =>
            {
                e.ToTable("IPRESS_Red", t => t.HasTrigger("TR_IPRESS_Red_Audit"));
                e.HasKey(x => x.IdRed);
                e.Property(x => x.IdRed).HasColumnName("ID_RED");
                e.Property(x => x.IdDiresa).HasColumnName("ID_DIRESA");
                e.Property(x => x.Codigo).HasColumnName("Codigo");
                e.Property(x => x.Ubigeo).HasMaxLength(6).HasColumnName("Ubigeo");
                e.HasOne(x => x.Diresa).WithMany(x => x.Redes).HasForeignKey(x => x.IdDiresa);
                e.HasOne(x => x.Distrito).WithMany().HasForeignKey(x => x.Ubigeo).IsRequired(false);
            });
            mb.Entity<IpressMicroRed>(e =>
            {
                e.ToTable("IPRESS_MicroRed", t => t.HasTrigger("TR_IPRESS_MicroRed_Audit"));
                e.HasKey(x => x.IdMicroRed);
                e.Property(x => x.IdMicroRed).HasColumnName("ID_MICRORED");
                e.Property(x => x.IdRed).HasColumnName("ID_RED");
                e.Property(x => x.Codigo).HasColumnName("Codigo");
                e.Property(x => x.Ubigeo).HasMaxLength(6).HasColumnName("Ubigeo");
                e.HasOne(x => x.Red).WithMany(x => x.MicroRedes).HasForeignKey(x => x.IdRed);
                e.HasOne(x => x.Distrito).WithMany().HasForeignKey(x => x.Ubigeo).IsRequired(false);
            });

            mb.Entity<IpressCentroPoblado>(e =>
            {
                e.ToTable("IPRESS_CentroPoblado");
                e.HasKey(x => x.IdCentroPoblado);
                e.Property(x => x.IdCentroPoblado).HasColumnName("ID_CENTRO_POBLADO");
                e.Property(x => x.Ubigeo).HasMaxLength(6).HasColumnName("Ubigeo");
                e.Property(x => x.IdEstablecimiento).HasColumnName("ID_ESTABLECIMIENTO");
                e.Property(x => x.Este).HasPrecision(18, 2);
                e.Property(x => x.Norte).HasPrecision(18, 2);
                e.Property(x => x.Latitud).HasPrecision(12, 6);
                e.Property(x => x.Longitud).HasPrecision(12, 6);
                e.Property(x => x.TempMinima).HasPrecision(6, 2);
                e.Property(x => x.TempMaxima).HasPrecision(6, 2);
                e.HasOne(x => x.Establecimiento).WithMany().HasForeignKey(x => x.IdEstablecimiento).IsRequired(false);
                e.HasMany(x => x.Accesibilidades).WithOne(a => a.CentroPoblado).HasForeignKey(a => a.IdCentroPoblado).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.CentrosEducativos).WithOne(c => c.CentroPoblado).HasForeignKey(c => c.IdCentroPoblado).OnDelete(DeleteBehavior.Cascade);
                e.HasMany(x => x.Autoridades).WithOne(a => a.CentroPoblado).HasForeignKey(a => a.IdCentroPoblado).OnDelete(DeleteBehavior.Cascade);
            });
            mb.Entity<IpressCentroPobladoAccesibilidad>(e =>
            {
                e.ToTable("IPRESS_CentroPobladoAccesibilidad");
                e.HasKey(x => x.IdAccesibilidad);
                e.Property(x => x.IdAccesibilidad).HasColumnName("ID_ACCESIBILIDAD");
                e.Property(x => x.IdCentroPoblado).HasColumnName("ID_CENTRO_POBLADO");
                e.Property(x => x.DistanciaKm).HasPrecision(10, 2);
            });
            mb.Entity<IpressCentroPobladoCentroEducativo>(e =>
            {
                e.ToTable("IPRESS_CentroPobladoCentroEducativo");
                e.HasKey(x => x.IdCentroEducativo);
                e.Property(x => x.IdCentroEducativo).HasColumnName("ID_CENTRO_EDUCATIVO");
                e.Property(x => x.IdCentroPoblado).HasColumnName("ID_CENTRO_POBLADO");
            });
            mb.Entity<IpressCentroPobladoAutoridad>(e =>
            {
                e.ToTable("IPRESS_CentroPobladoAutoridad");
                e.HasKey(x => x.IdAutoridad);
                e.Property(x => x.IdAutoridad).HasColumnName("ID_AUTORIDAD");
                e.Property(x => x.IdCentroPoblado).HasColumnName("ID_CENTRO_POBLADO");
            });

            mb.Entity<IpressEstablecimiento>(e =>
            {
                e.ToTable("IPRESS_Establecimiento", t => t.HasTrigger("TR_IPRESS_Establecimiento_Audit"));
                e.HasKey(x => x.IdEstablecimiento);
                e.Property(x => x.IdEstablecimiento).HasColumnName("ID_ESTABLECIMIENTO");
                e.Property(x => x.IdDiresa).HasColumnName("ID_DIRESA");
                e.Property(x => x.IdRed).HasColumnName("ID_RED");
                e.Property(x => x.IdMicroRed).HasColumnName("ID_MICRORED");
                e.Property(x => x.Este).HasPrecision(18, 2);
                e.Property(x => x.Norte).HasPrecision(18, 2);
                e.Property(x => x.Latitud).HasPrecision(12, 6);
                e.Property(x => x.Longitud).HasPrecision(12, 6);
                e.HasOne(x => x.Diresa).WithMany().HasForeignKey(x => x.IdDiresa);
                e.HasOne(x => x.Red).WithMany().HasForeignKey(x => x.IdRed);
                e.HasOne(x => x.MicroRed).WithMany().HasForeignKey(x => x.IdMicroRed);
            });

            mb.Entity<IpressEstablecimientoCentroPoblado>(e =>
            {
                e.ToTable("IPRESS_EstablecimientoCentroPoblado");
                e.HasKey(x => new { x.IdEstablecimiento, x.IdCentroPoblado });
                e.Property(x => x.IdEstablecimiento).HasColumnName("ID_ESTABLECIMIENTO");
                e.Property(x => x.IdCentroPoblado).HasColumnName("ID_CENTRO_POBLADO");
                e.HasOne(x => x.Establecimiento).WithMany(x => x.EstablecimientoCentrosPoblados).HasForeignKey(x => x.IdEstablecimiento);
                e.HasOne(x => x.CentroPoblado).WithMany().HasForeignKey(x => x.IdCentroPoblado);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _auditUserIdProvider?.GetUserId();
            var clientIp = _auditUserIdProvider?.GetClientIp();
            if (userId.HasValue || !string.IsNullOrEmpty(clientIp))
            {
                var bytes = new byte[128];
                if (userId.HasValue)
                {
                    var idBytes = BitConverter.GetBytes(userId.Value);
                    Array.Copy(idBytes, 0, bytes, 0, Math.Min(4, idBytes.Length));
                }
                if (!string.IsNullOrEmpty(clientIp))
                {
                    var ipBytes = System.Text.Encoding.ASCII.GetBytes(clientIp);
                    Array.Copy(ipBytes, 0, bytes, 4, Math.Min(45, ipBytes.Length));
                }
                var hex = "0x" + BitConverter.ToString(bytes).Replace("-", "", StringComparison.Ordinal);
#pragma warning disable EF1002 // hex es generado desde userId e IP de contexto; no hay riesgo de inyección SQL
                await Database.ExecuteSqlRawAsync($"SET CONTEXT_INFO {hex}", cancellationToken);
#pragma warning restore EF1002
            }
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
