using Microsoft.AspNetCore.Mvc;
using intern_task_2.DTOs;
using intern_task_2.Services;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ApartmentsController : ControllerBase
{
    private readonly IApartmentService _apartmentService;

    public ApartmentsController(IApartmentService apartmentService)
    {
        _apartmentService = apartmentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ApartmentDto>>> GetApartments()
    {
        var apartments = await _apartmentService.GetAllAsync();
        return Ok(apartments);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApartmentDto>> GetApartment(int id)
    {
        var apartment = await _apartmentService.GetByIdAsync(id);
        
        if (apartment == null)
            return NotFound(new { message = "Apartment not found" });
        
        return Ok(apartment);
    }

    [HttpPost]
    public async Task<ActionResult<ApartmentDto>> CreateApartment(ApartmentCreateDto dto)
    {
        try
        {
            var apartment = await _apartmentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetApartment), new { id = apartment.Id }, apartment);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateApartment(int id, ApartmentUpdateDto dto)
    {
        try
        {
            var success = await _apartmentService.UpdateAsync(id, dto);
            
            if (!success)
                return NotFound(new { message = "Apartment not found" });
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteApartment(int id)
    {
        var success = await _apartmentService.DeleteAsync(id);
        
        if (!success)
            return NotFound(new { message = "Apartment not found" });
        
        return NoContent();
    }
}
