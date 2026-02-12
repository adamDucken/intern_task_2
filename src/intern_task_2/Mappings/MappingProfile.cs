using AutoMapper;
using intern_task_2.Models;
using intern_task_2.DTOs;

namespace intern_task_2.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // House mappings
        CreateMap<House, HouseDto>();
        CreateMap<HouseCreateDto, House>();
        CreateMap<HouseUpdateDto, House>();

        // Apartment mappings
        CreateMap<Apartment, ApartmentDto>();
        CreateMap<ApartmentCreateDto, Apartment>();
        CreateMap<ApartmentUpdateDto, Apartment>();

        // Resident mappings
        CreateMap<Resident, ResidentDto>();
        CreateMap<ResidentCreateDto, Resident>()
            .ForMember(dest => dest.ApartmentResidents, opt => opt.Ignore());
        CreateMap<ResidentUpdateDto, Resident>()
            .ForMember(dest => dest.ApartmentResidents, opt => opt.Ignore());
    }
}
