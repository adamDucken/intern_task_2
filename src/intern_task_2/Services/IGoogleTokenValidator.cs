namespace intern_task_2.Services;

public interface IGoogleTokenValidator
{
    Task<GoogleTokenPayload?> ValidateAsync(string idToken);
}

public class GoogleTokenPayload
{
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
}
