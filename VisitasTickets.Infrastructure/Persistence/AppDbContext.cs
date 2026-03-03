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

                entity.ToTable("ADM_Personal");

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

                entity.ToTable("ADM_Usuario");

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

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }

}
