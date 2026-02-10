using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using intern_task_2.Controllers;
using intern_task_2.Models;
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
        var createdHouse = await response.Content.ReadFromJsonAsync<House>();

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
}
