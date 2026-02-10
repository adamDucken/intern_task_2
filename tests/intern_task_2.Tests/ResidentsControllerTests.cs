using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using intern_task_2.Controllers;
using intern_task_2.Models;
using Xunit;

namespace intern_task_2.Tests;

public class ResidentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ResidentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateResident_WithValidData_ReturnsCreatedResident()
    {
        // Arrange
        var house = new HouseCreateDto
        {
            Number = "20C",
            Street = "Elm Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1070"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<House>();

        var apartment = new ApartmentCreateDto
        {
            Number = "12",
            Floor = 4,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 50.0m,
            LivingArea = 35.0m,
            HouseId = createdHouse!.Id
        };
        var apartmentResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<Apartment>();

        var newResident = new ResidentCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            PersonalCode = "123456-78901",
            DateOfBirth = new DateTime(1990, 5, 15),
            Phone = "+37120000000",
            Email = "john.doe@example.com",
            ApartmentId = createdApartment!.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Residents", newResident);
        var createdResident = await response.Content.ReadFromJsonAsync<Resident>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdResident);
        Assert.Equal(newResident.FirstName, createdResident.FirstName);
        Assert.Equal(newResident.LastName, createdResident.LastName);
        Assert.Equal(newResident.PersonalCode, createdResident.PersonalCode);
        Assert.Equal(newResident.Email, createdResident.Email);
    }

    [Fact]
    public async Task CreateResident_WithInvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var house = new HouseCreateDto
        {
            Number = "25D",
            Street = "Pine Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1080"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<House>();

        var apartment = new ApartmentCreateDto
        {
            Number = "7",
            Floor = 1,
            RoomCount = 1,
            ResidentCount = 1,
            TotalArea = 40.0m,
            LivingArea = 30.0m,
            HouseId = createdHouse!.Id
        };
        var apartmentResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<Apartment>();

        var invalidResident = new ResidentCreateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            PersonalCode = "234567-89012",
            DateOfBirth = new DateTime(1985, 8, 20),
            Phone = "+37120000001",
            Email = "invalid-email-format",
            ApartmentId = createdApartment!.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Residents", invalidResident);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
