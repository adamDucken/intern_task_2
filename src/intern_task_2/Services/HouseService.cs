using AutoMapper;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.DTOs;
using intern_task_2.Models;

namespace intern_task_2.Services;

public class HouseService : IHouseService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public HouseService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<HouseDto>> GetAllAsync()
    {
        var houses = await _context.Houses
            .Include(h => h.Apartments)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<HouseDto>>(houses);
    }

    public async Task<HouseDto?> GetByIdAsync(int id)
    {
        var house = await _context.Houses
            .Include(h => h.Apartments)
            .FirstOrDefaultAsync(h => h.Id == id);
        
        return house == null ? null : _mapper.Map<HouseDto>(house);
    }

    public async Task<HouseDto> CreateAsync(HouseCreateDto dto)
    {
        ValidateHouseDto(dto);
        
        var house = _mapper.Map<House>(dto);
        
        _context.Houses.Add(house);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<HouseDto>(house);
    }

    public async Task<bool> UpdateAsync(int id, HouseUpdateDto dto)
    {
        ValidateHouseDto(dto);
        
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
            return false;
        
        _mapper.Map(dto, house);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var house = await _context.Houses.FindAsync(id);
        if (house == null)
            return false;
        
        _context.Houses.Remove(house);
        await _context.SaveChangesAsync();
        
        return true;
    }

    private void ValidateHouseDto(HouseCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
            throw new ArgumentException("Number is required");
        if (string.IsNullOrWhiteSpace(dto.Street))
            throw new ArgumentException("Street is required");
        if (string.IsNullOrWhiteSpace(dto.City))
            throw new ArgumentException("City is required");
        if (string.IsNullOrWhiteSpace(dto.Country))
            throw new ArgumentException("Country is required");
        if (string.IsNullOrWhiteSpace(dto.PostalCode))
            throw new ArgumentException("PostalCode is required");
    }

    private void ValidateHouseDto(HouseUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
            throw new ArgumentException("Number is required");
        if (string.IsNullOrWhiteSpace(dto.Street))
            throw new ArgumentException("Street is required");
        if (string.IsNullOrWhiteSpace(dto.City))
            throw new ArgumentException("City is required");
        if (string.IsNullOrWhiteSpace(dto.Country))
            throw new ArgumentException("Country is required");
        if (string.IsNullOrWhiteSpace(dto.PostalCode))
            throw new ArgumentException("PostalCode is required");
    }
}
