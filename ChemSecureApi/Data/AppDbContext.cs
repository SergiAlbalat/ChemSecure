using ChemSecureApi.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChemSecureApi.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public DbSet<Tank> Tanks { get; set; }
        public DbSet<Truck> Trucks { get; set; }
        public DbSet<Trip> Trips { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
