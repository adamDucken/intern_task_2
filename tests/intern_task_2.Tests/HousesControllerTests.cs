using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using intern_task_2.DTOs;
using intern_task_2.Data;
using Xunit;

namespace intern_task_2.Tests;

public class HousesControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HousesControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateHouse_WithValidData_ReturnsCreatedHouse()
    {
        // Arrange
        var newHouse = new HouseCreateDto
        {
            Number = "10A",
            Street = "Main Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1010"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Houses", newHouse);
        var createdHouse = await response.Content.ReadFromJsonAsync<HouseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(createdHouse);
        Assert.Equal(newHouse.Number, createdHouse.Number);
        Assert.Equal(newHouse.Street, createdHouse.Street);
        Assert.Equal(newHouse.City, createdHouse.City);
        Assert.Equal(newHouse.Country, createdHouse.Country);
        Assert.Equal(newHouse.PostalCode, createdHouse.PostalCode);
    }

    [Fact]
    public async Task CreateHouse_WithMissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var invalidHouse = new HouseCreateDto
        {
            Number = "",
            Street = "Main Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1010"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Houses", invalidHouse);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllHouses_ReturnsListOfHouses()
    {
        // Act
        var response = await _client.GetAsync("/api/Houses");
        var houses = await response.Content.ReadFromJsonAsync<List<HouseDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(houses);
        Assert.NotEmpty(houses);
    }

    [Fact]
    public async Task GetHouseById_WithValidId_ReturnsHouse()
    {
        // Arrange - Create a house first
        var newHouse = new HouseCreateDto
        {
            Number = "99Z",
            Street = "Test Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-9999"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Houses", newHouse);
        var createdHouse = await createResponse.Content.ReadFromJsonAsync<HouseDto>();

        // Act
        var response = await _client.GetAsync($"/api/Houses/{createdHouse!.Id}");
        var house = await response.Content.ReadFromJsonAsync<HouseDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(house);
        Assert.Equal(createdHouse.Id, house.Id);
        Assert.Equal(newHouse.Number, house.Number);
    }

    [Fact]
    public async Task UpdateHouse_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create a house first
        var newHouse = new HouseCreateDto
        {
            Number = "88X",
            Street = "Old Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-8888"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Houses", newHouse);
        var createdHouse = await createResponse.Content.ReadFromJsonAsync<HouseDto>();

        var updateDto = new HouseUpdateDto
        {
            Number = "88Y",
            Street = "New Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-8888"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Houses/{createdHouse!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteHouse_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create a house first
        var newHouse = new HouseCreateDto
        {
            Number = "77W",
            Street = "Delete Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-7777"
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Houses", newHouse);
        var createdHouse = await createResponse.Content.ReadFromJsonAsync<HouseDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/Houses/{createdHouse!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
