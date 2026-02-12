using AutoMapper;
using Microsoft.EntityFrameworkCore;
using intern_task_2.Data;
using intern_task_2.DTOs;
using intern_task_2.Models;
using System.Text.RegularExpressions;

namespace intern_task_2.Services;

public class ResidentService : IResidentService
{
    private readonly ApplicationDbContext _context;
    private readonly IMapper _mapper;

    public ResidentService(ApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ResidentDto>> GetAllAsync()
    {
        var residents = await _context.Residents
            .Include(r => r.ApartmentResidents)
                .ThenInclude(ar => ar.Apartment)
                    .ThenInclude(a => a.House)
            .ToListAsync();
        
        return _mapper.Map<IEnumerable<ResidentDto>>(residents);
    }

    public async Task<ResidentDto?> GetByIdAsync(int id)
    {
        var resident = await _context.Residents
            .Include(r => r.ApartmentResidents)
                .ThenInclude(ar => ar.Apartment)
                    .ThenInclude(a => a.House)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        return resident == null ? null : _mapper.Map<ResidentDto>(resident);
    }

    public async Task<ResidentDto> CreateAsync(ResidentCreateDto dto)
    {
        await ValidateResidentDtoAsync(dto);
        
        var resident = _mapper.Map<Resident>(dto);
        
        _context.Residents.Add(resident);
        await _context.SaveChangesAsync();

        // Add apartment relationships
        if (dto.Apartments != null && dto.Apartments.Any())
        {
            foreach (var apartmentDto in dto.Apartments)
            {
                var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == apartmentDto.ApartmentId);
                if (!apartmentExists)
                    throw new ArgumentException($"Apartment with ID {apartmentDto.ApartmentId} does not exist");
                
                var apartmentResident = new ApartmentResident
                {
                    ResidentId = resident.Id,
                    ApartmentId = apartmentDto.ApartmentId,
                    IsOwner = apartmentDto.IsOwner
                };
                _context.ApartmentResidents.Add(apartmentResident);
            }
            await _context.SaveChangesAsync();
        }
        
        return _mapper.Map<ResidentDto>(resident);
    }

    public async Task<bool> UpdateAsync(int id, ResidentUpdateDto dto)
    {
        await ValidateResidentDtoAsync(dto);
        
        var resident = await _context.Residents
            .Include(r => r.ApartmentResidents)
            .FirstOrDefaultAsync(r => r.Id == id);
        
        if (resident == null)
            return false;
        
        _mapper.Map(dto, resident);

        // Update apartment relationships
        _context.ApartmentResidents.RemoveRange(resident.ApartmentResidents);
        
        if (dto.Apartments != null && dto.Apartments.Any())
        {
            foreach (var apartmentDto in dto.Apartments)
            {
                var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == apartmentDto.ApartmentId);
                if (!apartmentExists)
                    throw new ArgumentException($"Apartment with ID {apartmentDto.ApartmentId} does not exist");
                
                var apartmentResident = new ApartmentResident
                {
                    ResidentId = resident.Id,
                    ApartmentId = apartmentDto.ApartmentId,
                    IsOwner = apartmentDto.IsOwner
                };
                _context.ApartmentResidents.Add(apartmentResident);
            }
        }
        
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var resident = await _context.Residents.FindAsync(id);
        if (resident == null)
            return false;
        
        _context.Residents.Remove(resident);
        await _context.SaveChangesAsync();
        
        return true;
    }

    private async Task ValidateResidentDtoAsync(ResidentCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ArgumentException("FirstName is required");
        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ArgumentException("LastName is required");
        if (string.IsNullOrWhiteSpace(dto.PersonalCode))
            throw new ArgumentException("PersonalCode is required");
        if (dto.DateOfBirth >= DateTime.UtcNow)
            throw new ArgumentException("DateOfBirth must be in the past");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ArgumentException("Phone is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (!IsValidEmail(dto.Email))
            throw new ArgumentException("Email format is invalid");
    }

    private async Task ValidateResidentDtoAsync(ResidentUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.FirstName))
            throw new ArgumentException("FirstName is required");
        if (string.IsNullOrWhiteSpace(dto.LastName))
            throw new ArgumentException("LastName is required");
        if (string.IsNullOrWhiteSpace(dto.PersonalCode))
            throw new ArgumentException("PersonalCode is required");
        if (dto.DateOfBirth >= DateTime.UtcNow)
            throw new ArgumentException("DateOfBirth must be in the past");
        if (string.IsNullOrWhiteSpace(dto.Phone))
            throw new ArgumentException("Phone is required");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("Email is required");
        if (!IsValidEmail(dto.Email))
            throw new ArgumentException("Email format is invalid");
    }

    private bool IsValidEmail(string email)
    {
        var emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        return emailRegex.IsMatch(email);
    }
}
