using Microsoft.AspNetCore.Mvc;
using intern_task_2.DTOs;
using intern_task_2.Services;

namespace intern_task_2.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _env;

    public AuthController(IAuthService authService, IWebHostEnvironment env)
    {
        _authService = authService;
        _env = env;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
    {
        try
        {
            var response = await _authService.RegisterAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        try
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST /api/auth/google
    // Angular sends: { "idToken": "<google id token>" }
    // backend verifies with google, does find-or-create, returns our jwt
    [HttpPost("google")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLogin(GoogleLoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.IdToken))
            return BadRequest(new { message = "idToken is required" });

        try
        {
            var response = await _authService.GoogleLoginAsync(dto.IdToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // POST /api/auth/google/test
    // development-only endpoint used by automated tests to simulate
    // what angular would do after receiving a real google id token.
    // accepts { "email": "..." } directly, skips google signature check.
    [HttpPost("google/test")]
    public async Task<ActionResult<AuthResponseDto>> GoogleLoginTest([FromBody] GoogleTestLoginDto dto)
    {
        if (!_env.IsDevelopment())
            return NotFound();

        if (string.IsNullOrWhiteSpace(dto.Email))
            return BadRequest(new { message = "email is required" });

        try
        {
            var response = await _authService.GoogleLoginByEmailAsync(dto.Email);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class GoogleTestLoginDto
{
    public string Email { get; set; } = string.Empty;
}
