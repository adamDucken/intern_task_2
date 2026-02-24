using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using intern_task_2.DTOs;
using intern_task_2.Services;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HousesController : ControllerBase
{
    private readonly IHouseService _houseService;

    public HousesController(IHouseService houseService)
    {
        _houseService = houseService;
    }

    [HttpGet]
    [Authorize(Roles = "Manager,Resident")]
    public async Task<ActionResult<IEnumerable<HouseDto>>> GetHouses()
    {
        var houses = await _houseService.GetAllAsync();
        return Ok(houses);
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Manager,Resident")]
    public async Task<ActionResult<HouseDto>> GetHouse(int id)
    {
        var house = await _houseService.GetByIdAsync(id);

        if (house == null)
            return NotFound(new { message = "house not found" });

        return Ok(house);
    }

    [HttpPost]
    [Authorize(Roles = "Manager")]
    public async Task<ActionResult<HouseDto>> CreateHouse(HouseCreateDto dto)
    {
        try
        {
            var house = await _houseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetHouse), new { id = house.Id }, house);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> UpdateHouse(int id, HouseUpdateDto dto)
    {
        try
        {
            var success = await _houseService.UpdateAsync(id, dto);

            if (!success)
                return NotFound(new { message = "house not found" });

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> DeleteHouse(int id)
    {
        var success = await _houseService.DeleteAsync(id);

        if (!success)
            return NotFound(new { message = "house not found" });

        return NoContent();
    }
}
