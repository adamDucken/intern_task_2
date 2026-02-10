using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.Models;
using System.Text.RegularExpressions;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResidentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ResidentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/Residents
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Resident>>> GetResidents()
    {
        return await _context.Residents
            .Include(r => r.Apartment)
            .ThenInclude(a => a!.House)
            .ToListAsync();
    }

    // GET: api/Residents/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Resident>> GetResident(int id)
    {
        var resident = await _context.Residents
            .Include(r => r.Apartment)
            .ThenInclude(a => a!.House)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (resident == null)
        {
            return NotFound(new { message = "Resident not found" });
        }

        return resident;
    }

    // POST: api/Residents
    [HttpPost]
    public async Task<ActionResult<Resident>> CreateResident(ResidentCreateDto dto)
    {
        if (!ValidateResidentDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == dto.ApartmentId);
        if (!apartmentExists)
        {
            return BadRequest(new { message = "Apartment does not exist" });
        }

        var resident = new Resident
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            PersonalCode = dto.PersonalCode,
            DateOfBirth = dto.DateOfBirth,
            Phone = dto.Phone,
            Email = dto.Email,
            ApartmentId = dto.ApartmentId
        };

        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetResident), new { id = resident.Id }, resident);
    }

    // PUT: api/Residents/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResident(int id, ResidentUpdateDto dto)
    {
        if (!ValidateResidentDto(dto, out var validationError))
        {
            return BadRequest(new { message = validationError });
        }

        var resident = await _context.Residents.FindAsync(id);
        if (resident == null)
        {
            return NotFound(new { message = "Resident not found" });
        }

        var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == dto.ApartmentId);
        if (!apartmentExists)
        {
            return BadRequest(new { message = "Apartment does not exist" });
        }

        resident.FirstName = dto.FirstName;
        resident.LastName = dto.LastName;
        resident.PersonalCode = dto.PersonalCode;
        resident.DateOfBirth = dto.DateOfBirth;
        resident.Phone = dto.Phone;
        resident.Email = dto.Email;
        resident.ApartmentId = dto.ApartmentId;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Residents/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResident(int id)
    {
        var resident = await _context.Residents.FindAsync(id);
        if (resident == null)
        {
            return NotFound(new { message = "Resident not found" });
        }

        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ValidateResidentDto(IResidentDto dto, out string error)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
        {
            error = "FirstName is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.LastName))
        {
            error = "LastName is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.PersonalCode))
        {
            error = "PersonalCode is required";
            return false;
        }
        if (dto.DateOfBirth >= DateTime.UtcNow)
        {
            error = "DateOfBirth must be in the past";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.Phone))
        {
            error = "Phone is required";
            return false;
        }
        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            error = "Email is required";
            return false;
        }
        if (!IsValidEmail(dto.Email))
        {
            error = "Email format is invalid";
            return false;
        }
        if (dto.ApartmentId <= 0)
        {
            error = "ApartmentId is required";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}

public interface IResidentDto
{
    string FirstName { get; }
    string LastName { get; }
    string PersonalCode { get; }
    DateTime DateOfBirth { get; }
    string Phone { get; }
    string Email { get; }
    int ApartmentId { get; }
}

public class ResidentCreateDto : IResidentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ApartmentId { get; set; }
}

public class ResidentUpdateDto : IResidentDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int ApartmentId { get; set; }
}
