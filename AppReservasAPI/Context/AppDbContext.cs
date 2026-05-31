using Microsoft.EntityFrameworkCore;

namespace AppReservasAPI.Context
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }
        public DbSet<AppReservasAPI.Models.Usuarios> Usuarios { get; set; }
        public DbSet<AppReservasAPI.Models.Roles> Roles { get; set; }
    }
}
