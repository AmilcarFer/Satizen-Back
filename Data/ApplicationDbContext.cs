using Microsoft.EntityFrameworkCore;
using Satizen_Api.Models;

namespace Satizen_Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } 
        public DbSet<Roles> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Paciente> Pacientes { get; set; } 
        public DbSet<Sector> Sectores { get; set; } 
        public DbSet<Personal> Personals { get; set; } 
        public DbSet<Institucion> Instituciones { get; set; } 
        public DbSet<DispositivoLaboral> DispositivosLaborales { get; set; } 
        public DbSet<Asignacion> Asignaciones { get; set; } 
        public DbSet<Contacto> Contactos { get; set; } 
        public DbSet<Llamado> Llamados { get; set; } 
        public DbSet<Mensaje> Mensajes { get; set; }
        public DbSet<Turno> Turnos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Permiso>().HasData(
                new Permiso() { idPermiso = 1, tipo = "Crear" },
                new Permiso() { idPermiso = 2, tipo = "Leer" },
                new Permiso() { idPermiso = 3, tipo = "Eliminar" },
                new Permiso() { idPermiso = 4, tipo = "Actualizar" }
            );

            modelBuilder.Entity<Roles>().HasData(
                new Roles() { idRol = 1, nombre = "Administrador", descripcion = "Soy administrador", idPermiso = 1 },
                new Roles() { idRol = 2, nombre = "Medico", descripcion = "Soy médico", idPermiso = 2 },
                new Roles() { idRol = 3, nombre = "Enfermero", descripcion = "Soy enfermero", idPermiso = 2 }
            );

            modelBuilder.Entity<Turno>().HasData(
                new Turno() { idTurno = 1, Nombre = "Mañana"},
                new Turno() { idTurno = 2, Nombre = "Tarde"},
                new Turno() { idTurno = 3, Nombre = "Noche"}
            );

            modelBuilder.Entity<Usuario>().HasData(
                new Usuario() { idUsuario = 1, nombreUsuario = "admin", correo = "admin@gmail.com", password = "123", idRoles = 1 }
                );  

            modelBuilder.Entity<RefreshToken>()
                .Property(o => o.esActivo)
                .HasComputedColumnSql("IIF(fechaExpiracion < GETDATE(), CONVERT(BIT, 0), CONVERT(BIT, 1))");

            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Autor)
                .WithMany()
                .HasForeignKey(m => m.idAutor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.Receptor)
                .WithMany()
                .HasForeignKey(m => m.idReceptor)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asignacion>()
                .HasOne(a => a.Personals)
                .WithMany()
                .HasForeignKey(a => a.idPersonal);

            modelBuilder.Entity<Asignacion>()
                .HasOne(a => a.Sectores)
                .WithMany()
                .HasForeignKey(a => a.idSector)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asignacion>()
                .HasOne(a => a.Turnos)
                .WithMany()
                .HasForeignKey(a => a.idTurno)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}
