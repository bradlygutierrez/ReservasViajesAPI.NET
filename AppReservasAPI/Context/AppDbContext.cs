using AppReservasAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Seguridad / usuarios
        public DbSet<Usuario> Usuarios { get; set; } = default!;
        public DbSet<Rol> Roles { get; set; } = default!;
        public DbSet<Pantalla> Pantallas { get; set; } = default!;
        public DbSet<Permiso> Permisos { get; set; } = default!;
        public DbSet<RolPantallaPermiso> RolPantallaPermisos { get; set; } = default!;

        // Catálogos geográficos
        public DbSet<Pais> Paises { get; set; } = default!;
        public DbSet<Ciudad> Ciudades { get; set; } = default!;
        public DbSet<Destino> Destinos { get; set; } = default!;

        // Viajes
        public DbSet<TipoViaje> TiposViaje { get; set; } = default!;
        public DbSet<Viaje> Viajes { get; set; } = default!;
        public DbSet<Disponibilidad> Disponibilidades { get; set; } = default!;

        // Reservas
        public DbSet<EstadoReserva> EstadosReserva { get; set; } = default!;
        public DbSet<Reserva> Reservas { get; set; } = default!;
        public DbSet<PasajeroReserva> PasajerosReserva { get; set; } = default!;
        public DbSet<HistorialEstadoReserva> HistorialEstadosReserva { get; set; } = default!;

        // Pagos
        public DbSet<EstadoPago> EstadosPago { get; set; } = default!;
        public DbSet<MetodoPago> MetodosPago { get; set; } = default!;
        public DbSet<Pago> Pagos { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Usuario -> Rol
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(u => u.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ciudad -> Pais
            modelBuilder.Entity<Ciudad>()
                .HasOne(c => c.Pais)
                .WithMany(p => p.Ciudades)
                .HasForeignKey(c => c.PaisId)
                .OnDelete(DeleteBehavior.Restrict);

            // Destino -> Ciudad
            modelBuilder.Entity<Destino>()
                .HasOne(d => d.Ciudad)
                .WithMany(c => c.Destinos)
                .HasForeignKey(d => d.CiudadId)
                .OnDelete(DeleteBehavior.Restrict);

            // Viaje -> TipoViaje
            modelBuilder.Entity<Viaje>()
                .HasOne(v => v.TipoViaje)
                .WithMany(tv => tv.Viajes)
                .HasForeignKey(v => v.TipoViajeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Viaje -> Destino
            modelBuilder.Entity<Viaje>()
                .HasOne(v => v.Destino)
                .WithMany(d => d.Viajes)
                .HasForeignKey(v => v.DestinoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Disponibilidad -> Viaje
            modelBuilder.Entity<Disponibilidad>()
                .HasOne(d => d.Viaje)
                .WithMany(v => v.Disponibilidades)
                .HasForeignKey(d => d.ViajeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva -> Usuario
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva -> Disponibilidad
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.Disponibilidad)
                .WithMany(d => d.Reservas)
                .HasForeignKey(r => r.DisponibilidadId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reserva -> EstadoReserva
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.EstadoReserva)
                .WithMany(er => er.Reservas)
                .HasForeignKey(r => r.EstadoReservaId)
                .OnDelete(DeleteBehavior.Restrict);

            // PasajeroReserva -> Reserva
            modelBuilder.Entity<PasajeroReserva>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.PasajerosReserva)
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Pago -> Reserva
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Reserva)
                .WithMany(r => r.Pagos)
                .HasForeignKey(p => p.ReservaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pago -> EstadoPago
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.EstadoPago)
                .WithMany(ep => ep.Pagos)
                .HasForeignKey(p => p.EstadoPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Pago -> MetodoPago
            modelBuilder.Entity<Pago>()
                .HasOne(p => p.MetodoPago)
                .WithMany(mp => mp.Pagos)
                .HasForeignKey(p => p.MetodoPagoId)
                .OnDelete(DeleteBehavior.Restrict);

            // HistorialEstadoReserva -> Reserva
            modelBuilder.Entity<HistorialEstadoReserva>()
                .HasOne(h => h.Reserva)
                .WithMany(r => r.HistorialEstadosReserva)
                .HasForeignKey(h => h.ReservaId)
                .OnDelete(DeleteBehavior.Cascade);

            // HistorialEstadoReserva -> Usuario
            modelBuilder.Entity<HistorialEstadoReserva>()
                .HasOne(h => h.Usuario)
                .WithMany(u => u.HistorialEstadoReservas)
                .HasForeignKey(h => h.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // HistorialEstadoReserva -> EstadoAnterior
            modelBuilder.Entity<HistorialEstadoReserva>()
                .HasOne(h => h.EstadoAnterior)
                .WithMany()
                .HasForeignKey(h => h.EstadoAnteriorId)
                .OnDelete(DeleteBehavior.Restrict);

            // HistorialEstadoReserva -> EstadoNuevo
            modelBuilder.Entity<HistorialEstadoReserva>()
                .HasOne(h => h.EstadoNuevo)
                .WithMany()
                .HasForeignKey(h => h.EstadoNuevoId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolPantallaPermiso -> Rol
            modelBuilder.Entity<RolPantallaPermiso>()
                .HasOne(rpp => rpp.Rol)
                .WithMany(r => r.RolPantallaPermisos)
                .HasForeignKey(rpp => rpp.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolPantallaPermiso -> Pantalla
            modelBuilder.Entity<RolPantallaPermiso>()
                .HasOne(rpp => rpp.Pantalla)
                .WithMany(p => p.RolPantallaPermisos)
                .HasForeignKey(rpp => rpp.PantallaId)
                .OnDelete(DeleteBehavior.Restrict);

            // RolPantallaPermiso -> Permiso
            modelBuilder.Entity<RolPantallaPermiso>()
                .HasOne(rpp => rpp.Permiso)
                .WithMany(p => p.RolPantallaPermisos)
                .HasForeignKey(rpp => rpp.PermisoId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RolPantallaPermiso>()
                .HasIndex(rpp => new { rpp.RolId, rpp.PantallaId, rpp.PermisoId })
                .IsUnique();
        }
    }
}