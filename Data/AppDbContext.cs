using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VacationPlanner.Web.Models.Domain;

namespace VacationPlanner.Web.Data
{
    public class AppDbContext : IdentityDbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        //Tables
        
        public DbSet<City> Cities { get; set; }
        public DbSet<WeatherRecord> WeatherRecords { get; set; }
        public DbSet<Attraction> Attractions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships City-Weather (one-to-many)
            modelBuilder.Entity<WeatherRecord>()
                .HasOne(w => w.City)
                .WithMany(c => c.WeatherRecords)
                .HasForeignKey(w => w.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure relationships City-Attraction (one-to-many)
            modelBuilder.Entity<Attraction>()
                .HasOne(a => a.City)
                .WithMany(c => c.Attractions)
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}