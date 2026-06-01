using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Context
{
    public class AppDbContext:DbContext
    {
        public DbSet<AppReservasAPI.Models.TIpoVIaje> TIpoVIaje { get; set; } = default!;
        public DbSet<AppReservasAPI.Models.Destinos> Destinos { get; set; } = default!;
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<AppReservasAPI.Models.Usuarios> Usuarios { get; set; }
        public DbSet<AppReservasAPI.Models.Roles> Roles { get; set; }

        public DbSet<AppReservasAPI.Models.Viaje> Viajes { get; set; }
    }
}
