using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using intern_task_2.Controllers;
using intern_task_2.Models;
using Xunit;

namespace intern_task_2.Tests;

public class ApartmentsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ApartmentsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateApartment_WithValidData_ReturnsCreatedApartment()
    {
        // Arrange
        var house = new HouseCreateDto
        {
            Number = "15B",
            Street = "Oak Avenue",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1050"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<House>();

        var newApartment = new ApartmentCreateDto
        {
            Number = "5",
            Floor = 2,
            RoomCount = 3,
            ResidentCount = 2,
            TotalArea = 75.5m,
            LivingArea = 55.0m,
            HouseId = createdHouse!.Id
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Apartments", newApartment);
        var createdApartment = await response.Content.ReadFromJsonAsync<Apartment>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdApartment);
        Assert.Equal(newApartment.Number, createdApartment.Number);
        Assert.Equal(newApartment.Floor, createdApartment.Floor);
        Assert.Equal(newApartment.RoomCount, createdApartment.RoomCount);
        Assert.Equal(newApartment.HouseId, createdApartment.HouseId);
    }

    [Fact]
    public async Task CreateApartment_WithInvalidHouseId_ReturnsBadRequest()
    {
        // Arrange
        var invalidApartment = new ApartmentCreateDto
        {
            Number = "10",
            Floor = 3,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 60.0m,
            LivingArea = 45.0m,
            HouseId = 99999
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Apartments", invalidApartment);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
