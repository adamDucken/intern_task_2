using intern_task_2.DTOs;

namespace intern_task_2.Services;

public interface IHouseService
{
    Task<IEnumerable<HouseDto>> GetAllAsync();
    Task<HouseDto?> GetByIdAsync(int id);
    Task<HouseDto> CreateAsync(HouseCreateDto dto);
    Task<bool> UpdateAsync(int id, HouseUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
