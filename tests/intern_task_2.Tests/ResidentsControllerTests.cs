using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using intern_task_2.DTOs;
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
        // Arrange - Create house and apartment first
        var house = new HouseCreateDto
        {
            Number = "20C",
            Street = "Elm Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1070"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

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
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var newResident = new ResidentCreateDto
        {
            FirstName = "John",
            LastName = "Doe",
            PersonalCode = "123456-78901",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 5, 15), DateTimeKind.Utc),
            Phone = "+37120000000",
            Email = "john.doe@example.com",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment!.Id,
                    IsOwner = true
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Residents", newResident);
        var createdResident = await response.Content.ReadFromJsonAsync<ResidentDto>();

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
        // Arrange - Create house and apartment first
        var house = new HouseCreateDto
        {
            Number = "25D",
            Street = "Pine Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1080"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

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
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var invalidResident = new ResidentCreateDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            PersonalCode = "234567-89012",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1985, 8, 20), DateTimeKind.Utc),
            Phone = "+37120000001",
            Email = "invalid-email-format",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment!.Id,
                    IsOwner = false
                }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/Residents", invalidResident);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetAllResidents_ReturnsListOfResidents()
    {
        // Act
        var response = await _client.GetAsync("/api/Residents");
        var residents = await response.Content.ReadFromJsonAsync<List<ResidentDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(residents);
        Assert.NotEmpty(residents);
    }

    [Fact]
    public async Task GetResidentById_WithValidId_ReturnsResident()
    {
        // Arrange - Create house, apartment and resident
        var house = new HouseCreateDto
        {
            Number = "33Q",
            Street = "Query Street",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-3333"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "15",
            Floor = 2,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 55.0m,
            LivingArea = 40.0m,
            HouseId = createdHouse!.Id
        };
        var apartmentResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var resident = new ResidentCreateDto
        {
            FirstName = "Test",
            LastName = "User",
            PersonalCode = "999999-99999",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1995, 3, 10), DateTimeKind.Utc),
            Phone = "+37129999999",
            Email = "test.user@example.com",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment!.Id,
                    IsOwner = true
                }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Residents", resident);
        var createdResident = await createResponse.Content.ReadFromJsonAsync<ResidentDto>();

        // Act
        var response = await _client.GetAsync($"/api/Residents/{createdResident!.Id}");
        var result = await response.Content.ReadFromJsonAsync<ResidentDto>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(result);
        Assert.Equal(createdResident.Id, result.Id);
    }

    [Fact]
    public async Task UpdateResident_WithValidData_ReturnsNoContent()
    {
        // Arrange - Create house, apartment and resident
        var house = new HouseCreateDto
        {
            Number = "22P",
            Street = "Update Road",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-2222"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "8",
            Floor = 1,
            RoomCount = 1,
            ResidentCount = 1,
            TotalArea = 35.0m,
            LivingArea = 25.0m,
            HouseId = createdHouse!.Id
        };
        var apartmentResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var resident = new ResidentCreateDto
        {
            FirstName = "Old",
            LastName = "Name",
            PersonalCode = "111111-11111",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1980, 1, 1), DateTimeKind.Utc),
            Phone = "+37111111111",
            Email = "old@example.com",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment!.Id,
                    IsOwner = false
                }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Residents", resident);
        var createdResident = await createResponse.Content.ReadFromJsonAsync<ResidentDto>();

        var updateDto = new ResidentUpdateDto
        {
            FirstName = "New",
            LastName = "Name",
            PersonalCode = "222222-22222",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1980, 1, 1), DateTimeKind.Utc),
            Phone = "+37122222222",
            Email = "new@example.com",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment.Id,
                    IsOwner = true
                }
            }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/Residents/{createdResident!.Id}", updateDto);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteResident_WithValidId_ReturnsNoContent()
    {
        // Arrange - Create house, apartment and resident
        var house = new HouseCreateDto
        {
            Number = "11O",
            Street = "Delete Boulevard",
            City = "Riga",
            Country = "Latvia",
            PostalCode = "LV-1111"
        };
        var houseResponse = await _client.PostAsJsonAsync("/api/Houses", house);
        var createdHouse = await houseResponse.Content.ReadFromJsonAsync<HouseDto>();

        var apartment = new ApartmentCreateDto
        {
            Number = "6",
            Floor = 3,
            RoomCount = 2,
            ResidentCount = 1,
            TotalArea = 60.0m,
            LivingArea = 45.0m,
            HouseId = createdHouse!.Id
        };
        var apartmentResponse = await _client.PostAsJsonAsync("/api/Apartments", apartment);
        var createdApartment = await apartmentResponse.Content.ReadFromJsonAsync<ApartmentDto>();

        var resident = new ResidentCreateDto
        {
            FirstName = "Delete",
            LastName = "Me",
            PersonalCode = "000000-00000",
            DateOfBirth = DateTime.SpecifyKind(new DateTime(1992, 12, 31), DateTimeKind.Utc),
            Phone = "+37100000000",
            Email = "delete@example.com",
            Apartments = new List<ApartmentResidentCreateDto>
            {
                new ApartmentResidentCreateDto
                {
                    ApartmentId = createdApartment!.Id,
                    IsOwner = true
                }
            }
        };
        var createResponse = await _client.PostAsJsonAsync("/api/Residents", resident);
        var createdResident = await createResponse.Content.ReadFromJsonAsync<ResidentDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/Residents/{createdResident!.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}
