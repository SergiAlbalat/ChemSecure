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
        public DbSet<Warning> Warnings { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Tank>()
                .HasOne(t => t.Client)
                .WithMany(u => u.Tanks)
                .HasForeignKey(t => t.ClientId)
                .IsRequired(false);             
        }
    }
}
