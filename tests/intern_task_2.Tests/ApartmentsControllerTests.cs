using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using intern_task_2.DTOs;
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
        // Arrange - Create a house first
        var house = new HouseCreateDto
        {
            Number = "15B",
            Street = "Oak Avenue",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1050"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

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
        var createdApartment = await response.Content.ReadFromJsonAsync<ApartmentDto>();

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

    [Fact]
    public async Task GetAllApartments_ReturnsListOfApartments()
    {
        // Act
        var response = await _client.GetAsync("/api/Apartments");
        var apartments = await response.Content.ReadFromJsonAsync<List<ApartmentDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(apartments);
        Assert.NotEmpty(apartments);
    }

    [Fact]
    public async Task GetApartmentById_WithValidId_ReturnsApartment()
    {
        // Arrange - Create house and apartment
        var house = new HouseCreateDto
        {
            Number = "66T",
            Street = "Test Avenue",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-6666"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "99",
            Floor = 5,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 50.0m,
            LivingArea = 35.0m,
            HouseId = createdHouse!.Id
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await createResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        // Act
        var response = await _client.GetAsync($"/api/Apartments/{createdApartment!.Id}");
        var result = await response.Content.ReadFromJsonAsync<ApartmentDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(createdApartment.Id, result.Id);
    }

    [Fact]
    public async Task UpdateApartment_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create house and apartment
        var house = new HouseCreateDto
        {
            Number = "55S",
            Street = "Update Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-5555"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "10",
            Floor = 1,
            RoomCount = 1,
            ResidentCount = 0,
            TotalArea = 30.0m,
            LivingArea = 20.0m,
            HouseId = createdHouse!.Id
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await createResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var updateDto = new ApartmentUpdateDto
        {
            Number = "11",
            Floor = 2,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 45.0m,
            LivingArea = 30.0m,
            HouseId = createdHouse.Id
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Apartments/{createdApartment!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteApartment_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create house and apartment
        var house = new HouseCreateDto
        {
            Number = "44R",
            Street = "Delete Avenue",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-4444"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "20",
            Floor = 3,
            RoomCount = 3,
            ResidentCount = 2,
            TotalArea = 70.0m,
            LivingArea = 50.0m,
            HouseId = createdHouse!.Id
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await createResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/Apartments/{createdApartment!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
