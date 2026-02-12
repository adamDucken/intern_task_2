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
    public DbSet<ApartmentResident> ApartmentResidents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // House configuration
        modelBuilder.Entity<House>(entity =>
        {
            entity.HasKey(h => h.Id);
            entity.Property(h => h.Number).IsRequired().HasMaxLength(50);
            entity.Property(h => h.Street).IsRequired().HasMaxLength(200);
            entity.Property(h => h.City).IsRequired().HasMaxLength(100);
            entity.Property(h => h.Country).IsRequired().HasMaxLength(100);
            entity.Property(h => h.PostalCode).IsRequired().HasMaxLength(20);
        });

        // Apartment configuration
        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Number).IsRequired().HasMaxLength(50);
            entity.Property(a => a.Floor).IsRequired();
            entity.Property(a => a.RoomCount).IsRequired();
            entity.Property(a => a.ResidentCount).IsRequired();
            entity.Property(a => a.TotalArea).IsRequired().HasColumnType("decimal(18,2)");
            entity.Property(a => a.LivingArea).IsRequired().HasColumnType("decimal(18,2)");
        });

        // Resident configuration
        modelBuilder.Entity<Resident>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.LastName).IsRequired().HasMaxLength(100);
            entity.Property(r => r.PersonalCode).IsRequired().HasMaxLength(50);
            entity.Property(r => r.DateOfBirth).IsRequired();
            entity.Property(r => r.Phone).IsRequired().HasMaxLength(50);
            entity.Property(r => r.Email).IsRequired().HasMaxLength(200);
        });

        // House-Apartment relationship (one-to-many)
        modelBuilder.Entity<House>()
            .HasMany(h => h.Apartments)
            .WithOne(a => a.House)
            .HasForeignKey(a => a.HouseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApartmentResident configuration (many-to-many)
        modelBuilder.Entity<ApartmentResident>(entity =>
        {
            entity.HasKey(ar => new { ar.ApartmentId, ar.ResidentId });

            entity.HasOne(ar => ar.Apartment)
                .WithMany(a => a.ApartmentResidents)
                .HasForeignKey(ar => ar.ApartmentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ar => ar.Resident)
                .WithMany(r => r.ApartmentResidents)
                .HasForeignKey(ar => ar.ResidentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(ar => ar.IsOwner).IsRequired();
        });
    }
}
