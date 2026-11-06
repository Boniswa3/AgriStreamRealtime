using AgriStreamRealtime.Models;
using Microsoft.EntityFrameworkCore;

namespace AgriStreamRealtime.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Service> Services { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<Role> Roles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Role>().HasData(
                new Role
                {
                    Id = 1,
                    RoleName = "Farmer"
                },
                new Role
                {
                    Id = 2,
                    RoleName = "Agronomist"
                },
                new Role
                {
                    Id = 3,
                    RoleName = "Admin"
                }
            );
        }
    }
}