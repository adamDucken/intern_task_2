using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.Models;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HousesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HousesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Houses
    [HttpGet]
    public async Task<ActionResult<IEnumerable<House>>> GetHouses()
    {
        return await _context.Houses.Include(h => h.Apartments).ToListAsync();
    }

    // GET: api/Houses/5
    [HttpGet("{id}")]
    public async Task<ActionResult<House>> GetHouse(int id)
    {
        var house = await _context.Houses
            .Include(h => h.Apartments)
            .FirstOrDefaultAsync(h => h.Id == id);

        if (house == null)
        {
            return NotFound(new { message = "House not found" });
        }

        return house;
    }

    // POST: api/Houses
    [HttpPost]
    public async Task<ActionResult<House>> CreateHouse(HouseCreateDto dto)
    {
        if (!ValidateHouseDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var house = new House
        {
            Number = dto.Number,
            Street = dto.Street,
            City = dto.City,
            Country = dto.Country,
            PostalCode = dto.PostalCode
        };

        _context.Houses.Add(house);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHouse), new { id = house.Id }, house);
    }

    // PUT: api/Houses/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateHouse(int id, HouseUpdateDto dto)
    {
        if (!ValidateHouseDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound(new { message = "House not found" });
        }

        house.Number = dto.Number;
        house.Street = dto.Street;
        house.City = dto.City;
        house.Country = dto.Country;
        house.PostalCode = dto.PostalCode;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Houses/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteHouse(int id)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
        {
            return NotFound(new { message = "House not found" });
        }

        _context.Houses.Remove(house);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ValidateHouseDto(IHouseDto dto, out string error)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
        {
            error = "Number is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.Street))
        {
            error = "Street is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.City))
        {
            error = "City is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.Country))
        {
            error = "Country is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.PostalCode))
        {
            error = "PostalCode is required";
            return false;
        }

        error = string.Empty;
        return true;
    }
}

public interface IHouseDto
{
    string Number { get; }
    string Street { get; }
    string City { get; }
    string Country { get; }
    string PostalCode { get; }
}

public class HouseCreateDto : IHouseDto
{
    public string Number { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}

public class HouseUpdateDto : IHouseDto
{
    public string Number { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
}
