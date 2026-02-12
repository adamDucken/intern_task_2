using intern_task_2.Models;

namespace intern_task_2.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Check if data already exists
        if (context.Houses.Any())
            return;

        // Create Houses
        var house1 = new House
        {
            Number = "12A",
            Street = "Brīvības iela",
            City = "Rīga",
            Country = "Latvia",
            PostalCode = "LV-1010"
        };

        var house2 = new House
        {
            Number = "45B",
            Street = "Elizabetes iela",
            City = "Rīga",
            Country = "Latvia",
            PostalCode = "LV-1050"
        };

        context.Houses.AddRange(house1, house2);
        await context.SaveChangesAsync();

        // Create Apartments
        var apartment1 = new Apartment
        {
            Number = "1",
            Floor = 1,
            RoomCount = 3,
            ResidentCount = 2,
            TotalArea = 75.50m,
            LivingArea = 55.30m,
            HouseId = house1.Id
        };

        var apartment2 = new Apartment
        {
            Number = "5",
            Floor = 2,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 55.00m,
            LivingArea = 40.00m,
            HouseId = house1.Id
        };

        var apartment3 = new Apartment
        {
            Number = "12",
            Floor = 5,
            RoomCount = 4,
            ResidentCount = 3,
            TotalArea = 95.75m,
            LivingArea = 70.25m,
            HouseId = house2.Id
        };

        var apartment4 = new Apartment
        {
            Number = "3",
            Floor = 1,
            RoomCount = 1,
            ResidentCount = 1,
            TotalArea = 35.00m,
            LivingArea = 25.00m,
            HouseId = house2.Id
        };

        var apartment5 = new Apartment
        {
            Number = "8",
            Floor = 3,
            RoomCount = 3,
            ResidentCount = 2,
            TotalArea = 80.00m,
            LivingArea = 60.00m,
            HouseId = house2.Id
        };

        context.Apartments.AddRange(apartment1, apartment2, apartment3, apartment4, apartment5);
        await context.SaveChangesAsync();

        // Create Residents - USE DateTime.SpecifyKind to set UTC
        var resident1 = new Resident
        {
            FirstName = "Jānis",
            LastName = "Bērziņš",
            PersonalCode = "010190-12345",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc),
            Phone = "+37120000001",
            Email = "janis.berzins@example.com"
        };

        var resident2 = new Resident
        {
            FirstName = "Anna",
            LastName = "Liepa",
            PersonalCode = "150585-54321",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 5, 15), DateTimeKind.Utc),
            Phone = "+37120000002",
            Email = "anna.liepa@example.com"
        };

        var resident3 = new Resident
        {
            FirstName = "Pēteris",
            LastName = "Kalniņš",
            PersonalCode = "221178-98765",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1978, 11, 22), DateTimeKind.Utc),
            Phone = "+37120000003",
            Email = "peteris.kalnins@example.com"
        };

        var resident4 = new Resident
        {
            FirstName = "Līga",
            LastName = "Ozola",
            PersonalCode = "080695-11111",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1995, 6, 8), DateTimeKind.Utc),
            Phone = "+37120000004",
            Email = "liga.ozola@example.com"
        };

        context.Residents.AddRange(resident1, resident2, resident3, resident4);
        await context.SaveChangesAsync();

        // Create ApartmentResident relationships
        var ar1 = new ApartmentResident
        {
            ApartmentId = apartment1.Id,
            ResidentId = resident1.Id,
            IsOwner = true
        };

        var ar2 = new ApartmentResident
        {
            ApartmentId = apartment1.Id,
            ResidentId = resident2.Id,
            IsOwner = false
        };

        var ar3 = new ApartmentResident
        {
            ApartmentId = apartment2.Id,
            ResidentId = resident2.Id,
            IsOwner = true
        };

        var ar4 = new ApartmentResident
        {
            ApartmentId = apartment3.Id,
            ResidentId = resident3.Id,
            IsOwner = true
        };

        var ar5 = new ApartmentResident
        {
            ApartmentId = apartment5.Id,
            ResidentId = resident4.Id,
            IsOwner = false
        };

        context.ApartmentResidents.AddRange(ar1, ar2, ar3, ar4, ar5);
        await context.SaveChangesAsync();
    }
}
