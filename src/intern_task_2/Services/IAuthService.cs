using intern_task_2.DTOs;

namespace intern_task_2.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
    Task<AuthResponseDto> GoogleLoginAsync(string idToken);
    Task<AuthResponseDto> GoogleLoginByEmailAsync(string email);
}
