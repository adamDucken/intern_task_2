using Microsoft.EntityFrameworkCore;
using intern_task_2.Models;

namespace intern_task_2.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<House> Houses { get; set; }
    public DbSet<Apartment> Apartments { get; set; }
    public DbSet<Resident> Residents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<House>()
            .HasMany(h => h.Apartments)
            .WithOne(a => a.House)
            .HasForeignKey(a => a.HouseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Apartment>()
            .HasMany(a => a.Residents)
            .WithOne(r => r.Apartment)
            .HasForeignKey(r => r.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
