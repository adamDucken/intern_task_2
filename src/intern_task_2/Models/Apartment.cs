namespace intern_task_2.Models;

public class Apartment
{
    public int Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int RoomCount { get; set; }
    public int ResidentCount { get; set; }
    public decimal TotalArea { get; set; }
    public decimal LivingArea { get; set; }
    
    public int HouseId { get; set; }
    public House? House { get; set; }
    
    public ICollection<ApartmentResident> ApartmentResidents { get; set; } = new List<ApartmentResident>();
}
