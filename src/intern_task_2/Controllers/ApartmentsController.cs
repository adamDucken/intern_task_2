using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.Models;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ApartmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Apartments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Apartment>>> GetApartments()
    {
        return await _context.Apartments
            .Include(a => a.House)
            .Include(a => a.Residents)
            .ToListAsync();
    }

    // GET: api/Apartments/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Apartment>> GetApartment(int id)
    {
        var apartment = await _context.Apartments
            .Include(a => a.House)
            .Include(a => a.Residents)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (apartment == null)
        {
            return NotFound(new { message = "Apartment not found" });
        }

        return apartment;
    }

    // POST: api/Apartments
    [HttpPost]
    public async Task<ActionResult<Apartment>> CreateApartment(ApartmentCreateDto dto)
    {
        if (!ValidateApartmentDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var houseExists = await _context.Houses.AnyAsync(h => h.Id == dto.HouseId);
        if (!houseExists)
        {
            return BadRequest(new { message = "House does not exist" });
        }

        var apartment = new Apartment
        {
            Number = dto.Number,
            Floor = dto.Floor,
            RoomCount = dto.RoomCount,
            ResidentCount = dto.ResidentCount,
            TotalArea = dto.TotalArea,
            LivingArea = dto.LivingArea,
            HouseId = dto.HouseId
        };

        _context.Apartments.Add(apartment);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetApartment), new { id = apartment.Id }, apartment);
    }

    // PUT: api/Apartments/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApartment(int id, ApartmentUpdateDto dto)
    {
        if (!ValidateApartmentDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null)
        {
            return NotFound(new { message = "Apartment not found" });
        }

        var houseExists = await _context.Houses.AnyAsync(h => h.Id == dto.HouseId);
        if (!houseExists)
        {
            return BadRequest(new { message = "House does not exist" });
        }

        apartment.Number = dto.Number;
        apartment.Floor = dto.Floor;
        apartment.RoomCount = dto.RoomCount;
        apartment.ResidentCount = dto.ResidentCount;
        apartment.TotalArea = dto.TotalArea;
        apartment.LivingArea = dto.LivingArea;
        apartment.HouseId = dto.HouseId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Apartments/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApartment(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null)
        {
            return NotFound(new { message = "Apartment not found" });
        }

        _context.Apartments.Remove(apartment);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ValidateApartmentDto(IApartmentDto dto, out string error)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
        {
            error = "Number is required";
            return false;
        }
        if (dto.Floor < 0)
        {
            error = "Floor must be non-negative";
            return false;
        }
        if (dto.RoomCount <= 0)
        {
            error = "RoomCount must be positive";
            return false;
        }
        if (dto.ResidentCount < 0)
        {
            error = "ResidentCount must be non-negative";
            return false;
        }
        if (dto.TotalArea <= 0)
        {
            error = "TotalArea must be positive";
            return false;
        }
        if (dto.LivingArea <= 0)
        {
            error = "LivingArea must be positive";
            return false;
        }
        if (dto.LivingArea > dto.TotalArea)
        {
            error = "LivingArea cannot exceed TotalArea";
            return false;
        }
        if (dto.HouseId <= 0)
        {
            error = "HouseId is required";
            return false;
        }

        error = string.Empty;
        return true;
    }
}

public interface IApartmentDto
{
    string Number { get; }
    int Floor { get; }
    int RoomCount { get; }
    int ResidentCount { get; }
    decimal TotalArea { get; }
    decimal LivingArea { get; }
    int HouseId { get; }
}

public class ApartmentCreateDto : IApartmentDto
{
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    public int HouseId { get; set; }
}

public class ApartmentUpdateDto : IApartmentDto
{
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    public int HouseId { get; set; }
}
