namespace intern_task_2.DTOs;

public class ApartmentDto
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    public int HouseId { get; set; }
}

public class ApartmentCreateDto
{
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    public int HouseId { get; set; }
}

public class ApartmentUpdateDto
{
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    public int HouseId { get; set; }
}
