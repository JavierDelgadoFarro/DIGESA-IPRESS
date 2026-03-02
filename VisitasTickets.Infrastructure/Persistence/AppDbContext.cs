using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using VisitasTickets.Domain.Entities;

namespace VisitasTickets.Infrastructure.Persistence
{

    public partial class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AdmArea> AdmAreas { get; set; }

        public virtual DbSet<AdmDetalleSubMenu> AdmDetalleSubMenus { get; set; }

        public virtual DbSet<AdmMenu> AdmMenus { get; set; }

        public virtual DbSet<AdmModulo> AdmModulos { get; set; }

        public virtual DbSet<AdmPersonal> AdmPersonals { get; set; }

        public virtual DbSet<AdmSubMenu> AdmSubMenus { get; set; }

        public virtual DbSet<AdmUsuario> AdmUsuarios { get; set; }

        // Atenciones
        public virtual DbSet<UtdAtencion> UtdAtencions { get; set; }

        public virtual DbSet<UtdHistorialAtencion> UtdHistorialAtencions { get; set; }

        public virtual DbSet<UtdEstadoAtencion> UtdEstadoAtencions { get; set; }

        public virtual DbSet<UtdTipoTramite> UtdTipoTramites { get; set; }

        public virtual DbSet<UtdTipoPreferencial> UtdTipoPreferencials { get; set; }

        public virtual DbSet<UtdTipoTrabajo> UtdTipoTrabajos { get; set; }

        public virtual DbSet<UtdDetalleActividad> UtdDetalleActividads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AdmArea>(entity =>
            {
                entity.HasKey(e => e.IdArea).HasName("PK_Area");

                entity.ToTable("ADM_Area");

                entity.Property(e => e.IdArea)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_AREA");
                entity.Property(e => e.AbreviaturasAre)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("AbreviaturasARE");
                entity.Property(e => e.DescripcionAre)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DescripcionARE");
                entity.Property(e => e.IdDireccion).HasColumnName("ID_DIRECCION");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.IdPersonal).HasColumnName("ID_PERSONAL");
                entity.Property(e => e.JefaturasAre)
                    .HasMaxLength(1)
                    .IsUnicode(false)
                    .HasColumnName("JefaturasARE");
            });

            modelBuilder.Entity<AdmDetalleSubMenu>(entity =>
            {
                entity.HasKey(e => e.IdDsubmenu);

                entity.ToTable("ADM_Detalle_Sub_Menu");

                entity.Property(e => e.IdDsubmenu)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_DSUBMENU");
                entity.Property(e => e.IdSmenu).HasColumnName("ID_SMENU");
                entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");

                entity.HasOne(d => d.IdSmenuNavigation).WithMany(p => p.AdmDetalleSubMenus)
                    .HasForeignKey(d => d.IdSmenu)
                    .HasConstraintName("FK_ADM_Detalle_Sub_Menu_ADM_Sub_Menu");
                entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.AdmDetalleSubMenus)
                    .HasForeignKey(d => d.IdUsuario)
                    .HasConstraintName("FK_ADM_Detalle_Sub_Menu_ADM_Usuario");
            });

            modelBuilder.Entity<AdmMenu>(entity =>
            {
                entity.HasKey(e => e.IdMenu);

                entity.ToTable("ADM_Menu");

                entity.Property(e => e.IdMenu)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_MENU");
                entity.Property(e => e.DescripcionMen)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DescripcionMEN");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.IdModulo).HasColumnName("ID_MODULO");
                entity.Property(e => e.OrdenMen).HasColumnName("OrdenMEN");

                entity.HasOne(d => d.IdModuloNavigation).WithMany(p => p.AdmMenus)
                    .HasForeignKey(d => d.IdModulo)
                    .HasConstraintName("FK_ADM_Menu_ADM_Modulo");
            });

            modelBuilder.Entity<AdmModulo>(entity =>
            {
                entity.HasKey(e => e.IdModulo);

                entity.ToTable("ADM_Modulo");

                entity.Property(e => e.IdModulo)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_MODULO");
                entity.Property(e => e.DescripcionMod)
                    .HasMaxLength(60)
                    .IsUnicode(false)
                    .HasColumnName("DescripcionMOD");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.Identificador)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<AdmPersonal>(entity =>
            {
                entity.HasKey(e => e.IdPersonal).HasName("PK_Personal");

                entity.ToTable("ADM_Personal", tb => tb.HasTrigger("ADM_Personal_ChangeTracking"));

                entity.HasIndex(e => e.ApellidosNombrePer, "IndNcApellidosNombresper29092018").HasFillFactor(90);

                entity.HasIndex(e => e.IdPersonal, "_dta_index_ADM_Personal_5_860582154__K1_7_29092018").HasFillFactor(90);

                entity.Property(e => e.IdPersonal)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_PERSONAL");
                entity.Property(e => e.AlergiaPer)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("AlergiaPER");
                entity.Property(e => e.AnexoPer)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("AnexoPER");
                entity.Property(e => e.ApellidosNombrePer)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("ApellidosNombrePER");
                entity.Property(e => e.Area)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("AREA");
                entity.Property(e => e.CargoMinsa)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.CondicionLaboralPer)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("CondicionLaboralPER");
                entity.Property(e => e.ContactoEmergenciaPer)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("ContactoEmergenciaPER");
                entity.Property(e => e.Direccion)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DIRECCION");
                entity.Property(e => e.DistritoPer)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("DistritoPER");
                entity.Property(e => e.Dniper)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("DNIPER");
                entity.Property(e => e.DomicilioPer)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("DomicilioPER");
                entity.Property(e => e.EmailPer)
                    .HasMaxLength(400)
                    .IsUnicode(false)
                    .HasColumnName("EmailPER");
                entity.Property(e => e.FechaIngresoPer)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaIngresoPER");
                entity.Property(e => e.FechaNacimientoPer)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaNacimientoPER");
                entity.Property(e => e.FechaRegistroPer)
                    .HasColumnType("datetime")
                    .HasColumnName("FechaRegistroPER");
                entity.Property(e => e.IdArea).HasColumnName("ID_AREA");
                entity.Property(e => e.IdCargo).HasColumnName("ID_CARGO");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.IdProfesion).HasColumnName("ID_PROFESION");
                entity.Property(e => e.IdSede).HasColumnName("ID_SEDE");
                entity.Property(e => e.IdTemp).HasColumnName("ID_TEMP");
                entity.Property(e => e.IdUsuarioper).HasColumnName("ID_USUARIOPER");
                entity.Property(e => e.Iniciales)
                    .HasMaxLength(10)
                    .IsUnicode(false);
                entity.Property(e => e.Materno)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.NumeroSeguroPer)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NumeroSeguroPER");
                entity.Property(e => e.ObservaccionPer)
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnName("ObservaccionPER");
                entity.Property(e => e.Paterno)
                    .HasMaxLength(100)
                    .IsUnicode(false);
                entity.Property(e => e.PrestacionSaludPer)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("PrestacionSaludPER");
                entity.Property(e => e.RefPer)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("RefPER");
                entity.Property(e => e.RegimenPensionPer)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("RegimenPensionPER");
                entity.Property(e => e.TelefonoPer)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("TelefonoPER");
                entity.Property(e => e.TipoSangrePer)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("TipoSangrePER");

                entity.HasOne(d => d.IdAreaNavigation).WithMany(p => p.AdmPersonals)
                    .HasForeignKey(d => d.IdArea)
                    .HasConstraintName("FK_Personal_Area");
            });

            modelBuilder.Entity<AdmSubMenu>(entity =>
            {
                entity.HasKey(e => e.IdSmenu);

                entity.ToTable("ADM_Sub_Menu");

                entity.Property(e => e.IdSmenu)
                    .ValueGeneratedNever()
                    .HasColumnName("ID_SMENU");
                entity.Property(e => e.DescripcionSme)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("DescripcionSME");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.IdMenu).HasColumnName("ID_MENU");
                entity.Property(e => e.OrdenSme).HasColumnName("OrdenSME");
                entity.Property(e => e.RutaWebSme)
                    .HasMaxLength(100)
                    .HasColumnName("RutaWebSME");

                entity.HasOne(d => d.IdMenuNavigation).WithMany(p => p.AdmSubMenus)
                    .HasForeignKey(d => d.IdMenu)
                    .HasConstraintName("FK_ADM_Sub_Menu_ADM_Menu");
            });

            modelBuilder.Entity<AdmUsuario>(entity =>
            {
                entity.HasKey(e => e.IdUsuario).HasName("PK_Usuario");

                entity.ToTable("ADM_Usuario", tb => tb.HasTrigger("ADM_Usuario_ChangeTracking"));

                entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
                entity.Property(e => e.ContrasenaUsu)
                    .HasMaxLength(12)
                    .IsUnicode(false)
                    .HasColumnName("ContrasenaUSU");
                entity.Property(e => e.IdArea).HasColumnName("ID_AREA");
                entity.Property(e => e.IdEstado).HasColumnName("ID_ESTADO");
                entity.Property(e => e.IdPersonal).HasColumnName("ID_PERSONAL");
                entity.Property(e => e.IdSede).HasColumnName("ID_SEDE");
                entity.Property(e => e.NombreUsuarioUsu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("NombreUsuarioUSU");

                entity.HasOne(d => d.IdPersonalNavigation).WithMany(p => p.AdmUsuarios)
                    .HasForeignKey(d => d.IdPersonal)
                    .HasConstraintName("FK_Usuario_Personal");
            });

            // Configuración de Atenciones
            modelBuilder.Entity<UtdEstadoAtencion>(entity =>
            {
                entity.HasKey(e => e.IdEstadoAtencion);

                entity.ToTable("UTD_ESTADO_ATENCION");

                entity.Property(e => e.IdEstadoAtencion).HasColumnName("ID_ESTADO_ATENCION");
                entity.Property(e => e.NombreEstado)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_ESTADO");
                entity.Property(e => e.Orden).HasColumnName("ORDEN");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
            });

            modelBuilder.Entity<UtdTipoTramite>(entity =>
            {
                entity.HasKey(e => e.IdTipoTramite);

                entity.ToTable("UTD_TIPO_TRAMITE");

                entity.Property(e => e.IdTipoTramite).HasColumnName("ID_TIPO_TRAMITE");
                entity.Property(e => e.NombreTramite)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_TRAMITE");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
            });

            modelBuilder.Entity<UtdTipoPreferencial>(entity =>
            {
                entity.HasKey(e => e.IdTipoPreferencial);

                entity.ToTable("UTD_TIPO_PREFERENCIAL");

                entity.Property(e => e.IdTipoPreferencial).HasColumnName("ID_TIPO_PREFERENCIAL");
                entity.Property(e => e.NombreTipoPreferencial)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_TIPO_PREFERENCIAL");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
            });

            modelBuilder.Entity<UtdTipoTrabajo>(entity =>
            {
                entity.HasKey(e => e.IdTipoTrabajo);

                entity.ToTable("UTD_TIPO_TRABAJO");

                entity.Property(e => e.IdTipoTrabajo).HasColumnName("ID_TIPO_TRABAJO");
                entity.Property(e => e.NombreTipoTrabajo)
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_TIPO_TRABAJO");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
            });

            modelBuilder.Entity<UtdDetalleActividad>(entity =>
            {
                entity.HasKey(e => e.IdDetalleActividad);

                entity.ToTable("UTD_DETALLE_ACTIVIDAD");

                entity.Property(e => e.IdDetalleActividad).HasColumnName("ID_DETALLE_ACTIVIDAD");
                entity.Property(e => e.NombreActividad)
                    .HasMaxLength(200)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRE_ACTIVIDAD");
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("DESCRIPCION");
                entity.Property(e => e.Estado).HasColumnName("ESTADO");
                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CREACION");
            });

            modelBuilder.Entity<UtdAtencion>(entity =>
            {
                entity.HasKey(e => e.IdAtencion);

                entity.ToTable("UTD_ATENCIONES");

                entity.HasIndex(e => e.IdEstadoAtencion, "IX_Atenciones_Estado");
                entity.HasIndex(e => e.FechaRegistro, "IX_Atenciones_FechaRegistro");
                entity.HasIndex(e => e.NumeroDocumento, "IX_Atenciones_NumeroDocumento");
                entity.HasIndex(e => e.IdTipoTramite, "IX_Atenciones_TipoTramite");

                entity.Property(e => e.IdAtencion).HasColumnName("ID_ATENCION");
                entity.Property(e => e.TipoDocumento)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("TIPO_DOCUMENTO");
                entity.Property(e => e.NumeroDocumento)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("NUMERO_DOCUMENTO");
                entity.Property(e => e.Nombres)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("NOMBRES");
                entity.Property(e => e.Apellidos)
                    .HasMaxLength(150)
                    .IsUnicode(false)
                    .HasColumnName("APELLIDOS");
                entity.Property(e => e.IdTipoTramite).HasColumnName("ID_TIPO_TRAMITE");
                entity.Property(e => e.Observacion)
                    .HasMaxLength(500)
                    .IsUnicode(false)
                    .HasColumnName("OBSERVACION");
                entity.Property(e => e.ObservacionAtencion)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("OBSERVACION_ATENCION");
                entity.Property(e => e.EsPreferencial).HasColumnName("ES_PREFERENCIAL");
                entity.Property(e => e.IdTipoPreferencial).HasColumnName("ID_TIPO_PREFERENCIAL");
                entity.Property(e => e.IdEstadoAtencion).HasColumnName("ID_ESTADO_ATENCION");
                entity.Property(e => e.IdTipoTrabajo).HasColumnName("ID_TIPO_TRABAJO");
                entity.Property(e => e.IdDetalleActividad).HasColumnName("ID_DETALLE_ACTIVIDAD");
                entity.Property(e => e.NumeroExpediente)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("NUMERO_EXPEDIENTE");
                entity.Property(e => e.FechaRegistro)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_REGISTRO");
                entity.Property(e => e.FechaActualizacion)
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_ACTUALIZACION");
                entity.Property(e => e.IdUsuarioRegistro).HasColumnName("ID_USUARIO_REGISTRO");
                entity.Property(e => e.IdUsuarioActualiza).HasColumnName("ID_USUARIO_ACTUALIZA");

                entity.HasOne(d => d.IdTipoTramiteNavigation).WithMany(p => p.UtdAtencions)
                    .HasForeignKey(d => d.IdTipoTramite)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Atenciones_TipoTramite");

                entity.HasOne(d => d.IdTipoPreferencialNavigation).WithMany(p => p.UtdAtencions)
                    .HasForeignKey(d => d.IdTipoPreferencial)
                    .HasConstraintName("FK_Atenciones_TipoPreferencial");

                entity.HasOne(d => d.IdEstadoAtencionNavigation).WithMany(p => p.UtdAtencions)
                    .HasForeignKey(d => d.IdEstadoAtencion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Atenciones_EstadoAtencion");

                entity.HasOne(d => d.IdTipoTrabajoNavigation).WithMany(p => p.UtdAtencions)
                    .HasForeignKey(d => d.IdTipoTrabajo)
                    .HasConstraintName("FK_Atenciones_TipoTrabajo");

                entity.HasOne(d => d.IdDetalleActividadNavigation).WithMany(p => p.UtdAtencions)
                    .HasForeignKey(d => d.IdDetalleActividad)
                    .HasConstraintName("FK_Atenciones_DetalleActividad");

                entity.HasOne(d => d.IdUsuarioRegistroNavigation).WithMany()
                    .HasForeignKey(d => d.IdUsuarioRegistro)
                    .HasConstraintName("FK_Atenciones_UsuarioRegistro");

                entity.HasOne(d => d.IdUsuarioActualizaNavigation).WithMany()
                    .HasForeignKey(d => d.IdUsuarioActualiza)
                    .HasConstraintName("FK_Atenciones_UsuarioActualiza");
            });

            modelBuilder.Entity<UtdHistorialAtencion>(entity =>
            {
                entity.HasKey(e => e.IdHistorial);

                entity.ToTable("UTD_HISTORIAL_ATENCIONES");

                entity.HasIndex(e => new { e.IdAtencion, e.FechaCambio }, "IX_HISTORIAL_ATENCION");
                entity.HasIndex(e => new { e.IdEstadoNuevo, e.FechaCambio }, "IX_HISTORIAL_ESTADO");

                entity.Property(e => e.IdHistorial).HasColumnName("ID_HISTORIAL");
                entity.Property(e => e.IdAtencion).HasColumnName("ID_ATENCION");
                entity.Property(e => e.IdEstadoAnterior).HasColumnName("ID_ESTADO_ANTERIOR");
                entity.Property(e => e.IdEstadoNuevo).HasColumnName("ID_ESTADO_NUEVO");
                entity.Property(e => e.IdUsuario).HasColumnName("ID_USUARIO");
                entity.Property(e => e.FechaCambio)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("FECHA_CAMBIO");
                entity.Property(e => e.Observacion)
                    .HasMaxLength(1000)
                    .IsUnicode(false)
                    .HasColumnName("OBSERVACION");
                entity.Property(e => e.TiempoEnEstadoAnterior).HasColumnName("TIEMPO_EN_ESTADO_ANTERIOR");

                entity.HasOne(d => d.IdAtencionNavigation).WithMany()
                    .HasForeignKey(d => d.IdAtencion)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HISTORIAL_ATENCION");

                entity.HasOne(d => d.IdEstadoAnteriorNavigation).WithMany()
                    .HasForeignKey(d => d.IdEstadoAnterior)
                    .HasConstraintName("FK_HISTORIAL_ESTADO_ANTERIOR");

                entity.HasOne(d => d.IdEstadoNuevoNavigation).WithMany()
                    .HasForeignKey(d => d.IdEstadoNuevo)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HISTORIAL_ESTADO_NUEVO");

                entity.HasOne(d => d.IdUsuarioNavigation).WithMany()
                    .HasForeignKey(d => d.IdUsuario)
                    .HasConstraintName("FK_HISTORIAL_USUARIO");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
