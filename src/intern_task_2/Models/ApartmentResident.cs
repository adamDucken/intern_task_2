namespace intern_task_2.Models;

public class ApartmentResident
{
    public int ApartmentId { get; set; }
    public Apartment Apartment { get; set; } = null!;
    
    public int ResidentId { get; set; }
    public Resident Resident { get; set; } = null!;
    
    public bool IsOwner { get; set; }
}
