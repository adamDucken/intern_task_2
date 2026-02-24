namespace intern_task_2.Models;

public class AppUser
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash { get; set; }
    public string Role { get; set; } = string.Empty;
    public string AuthProvider { get; set; } = "local";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
