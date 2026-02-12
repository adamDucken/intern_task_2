using intern_task_2.DTOs;

namespace intern_task_2.Services;

public interface IResidentService
{
    Task<IEnumerable<ResidentDto>> GetAllAsync();
    Task<ResidentDto?> GetByIdAsync(int id);
    Task<ResidentDto> CreateAsync(ResidentCreateDto dto);
    Task<bool> UpdateAsync(int id, ResidentUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
