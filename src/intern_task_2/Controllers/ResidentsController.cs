using Microsoft.AspNetCore.Mvc;
using intern_task_2.DTOs;
using intern_task_2.Services;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResidentsController : ControllerBase
{
    private readonly IResidentService _residentService;

    public ResidentsController(IResidentService residentService)
    {
        _residentService = residentService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ResidentDto>>> GetResidents()
    {
        var residents = await _residentService.GetAllAsync();
        return Ok(residents);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ResidentDto>> GetResident(int id)
    {
        var resident = await _residentService.GetByIdAsync(id);
        
        if (resident == null)
            return NotFound(new { message = "Resident not found" });
        
        return Ok(resident);
    }

    [HttpPost]
    public async Task<ActionResult<ResidentDto>> CreateResident(ResidentCreateDto dto)
    {
        try
        {
            var resident = await _residentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetResident), new { id = resident.Id }, resident);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateResident(int id, ResidentUpdateDto dto)
    {
        try
        {
            var success = await _residentService.UpdateAsync(id, dto);
            
            if (!success)
                return NotFound(new { message = "Resident not found" });
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteResident(int id)
    {
        var success = await _residentService.DeleteAsync(id);
        
        if (!success)
            return NotFound(new { message = "Resident not found" });
        
        return NoContent();
    }
}
