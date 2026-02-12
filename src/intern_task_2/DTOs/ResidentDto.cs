namespace intern_task_2.DTOs;

public class ResidentDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ResidentCreateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<ApartmentResidentCreateDto> Apartments { get; set; } = new List<ApartmentResidentCreateDto>();
}

public class ResidentUpdateDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PersonalCode { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<ApartmentResidentCreateDto> Apartments { get; set; } = new List<ApartmentResidentCreateDto>();
}

public class ApartmentResidentCreateDto
{
    public int ApartmentId { get; set; }
    public bool IsOwner { get; set; }
}
