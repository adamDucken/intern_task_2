using intern_task_2.DTOs;

namespace intern_task_2.Services;

public interface IApartmentService
{
    Task<IEnumerable<ApartmentDto>> GetAllAsync();
    Task<ApartmentDto?> GetByIdAsync(int id);
    Task<ApartmentDto> CreateAsync(ApartmentCreateDto dto);
    Task<bool> UpdateAsync(int id, ApartmentUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
