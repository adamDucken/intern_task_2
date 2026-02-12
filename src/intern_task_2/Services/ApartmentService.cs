using AutoMapper;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.DTOs;
using intern_task_2.Models;

namespace intern_task_2.Services;

public class ApartmentService : IApartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ApartmentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ApartmentDto>> GetAllAsync()
    {
        var apartments = await _context.Apartments
            .Include(a => a.House)
            .Include(a => a.ApartmentResidents)
                .ThenInclude(ar => ar.Resident)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<ApartmentDto>>(apartments);
    }

    public async Task<ApartmentDto?> GetByIdAsync(int id)
    {
        var apartment = await _context.Apartments
            .Include(a => a.House)
            .Include(a => a.ApartmentResidents)
                .ThenInclude(ar => ar.Resident)
            .FirstOrDefaultAsync(a => a.Id == id);
        
        return apartment == null ? null : _mapper.Map<ApartmentDto>(apartment);
    }

    public async Task<ApartmentDto> CreateAsync(ApartmentCreateDto dto)
    {
        await ValidateApartmentDtoAsync(dto);
        
        var apartment = _mapper.Map<Apartment>(dto);
        
        _context.Apartments.Add(apartment);
        await _context.SaveChangesAsync();
        
        return _mapper.Map<ApartmentDto>(apartment);
    }

    public async Task<bool> UpdateAsync(int id, ApartmentUpdateDto dto)
    {
        await ValidateApartmentDtoAsync(dto);
        
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null)
            return false;
        
        _mapper.Map(dto, apartment);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var apartment = await _context.Apartments.FindAsync(id);
        if (apartment == null)
            return false;
        
        _context.Apartments.Remove(apartment);
        await _context.SaveChangesAsync();
        
        return true;
    }

    private async Task ValidateApartmentDtoAsync(ApartmentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
            throw new ArgumentException("Number is required");
        if (dto.Floor < 0)
            throw new ArgumentException("Floor must be non-negative");
        if (dto.RoomCount <= 0)
            throw new ArgumentException("RoomCount must be positive");
        if (dto.ResidentCount < 0)
            throw new ArgumentException("ResidentCount must be non-negative");
        if (dto.TotalArea <= 0)
            throw new ArgumentException("TotalArea must be positive");
        if (dto.LivingArea <= 0)
            throw new ArgumentException("LivingArea must be positive");
        if (dto.LivingArea > dto.TotalArea)
            throw new ArgumentException("LivingArea cannot exceed TotalArea");
        if (dto.HouseId <= 0)
            throw new ArgumentException("HouseId is required");
        
        var houseExists = await _context.Houses.AnyAsync(h => h.Id == dto.HouseId);
        if (!houseExists)
            throw new ArgumentException("House does not exist");
    }

    private async Task ValidateApartmentDtoAsync(ApartmentUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Number))
            throw new ArgumentException("Number is required");
        if (dto.Floor < 0)
            throw new ArgumentException("Floor must be non-negative");
        if (dto.RoomCount <= 0)
            throw new ArgumentException("RoomCount must be positive");
        if (dto.ResidentCount < 0)
            throw new ArgumentException("ResidentCount must be non-negative");
        if (dto.TotalArea <= 0)
            throw new ArgumentException("TotalArea must be positive");
        if (dto.LivingArea <= 0)
            throw new ArgumentException("LivingArea must be positive");
        if (dto.LivingArea > dto.TotalArea)
            throw new ArgumentException("LivingArea cannot exceed TotalArea");
        if (dto.HouseId <= 0)
            throw new ArgumentException("HouseId is required");
        
        var houseExists = await _context.Houses.AnyAsync(h => h.Id == dto.HouseId);
        if (!houseExists)
            throw new ArgumentException("House does not exist");
    }
}
