using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using intern_task_2.Data;
using intern_task_2.DTOs;
using intern_task_2.Models;

namespace intern_task_2.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IGoogleTokenValidator _googleTokenValidator;

    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration,
        IGoogleTokenValidator googleTokenValidator)
    {
        _context = context;
        _configuration = configuration;
        _googleTokenValidator = googleTokenValidator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("password is required");

        if (dto.Password.Length < 6)
            throw new ArgumentException("password must be at least 6 characters");

        var allowedRoles = new[] { "Manager", "Resident" };
        if (!allowedRoles.Contains(dto.Role))
            throw new ArgumentException("role must be Manager or Resident");

        var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email.ToLower().Trim());
        if (existingUser)
            throw new ArgumentException("email is already registered");

        var user = new AppUser
        {
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            AuthProvider = "local",
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new ArgumentException("email is required");

        if (string.IsNullOrWhiteSpace(dto.Password))
            throw new ArgumentException("password is required");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower().Trim());

        if (user == null)
            throw new UnauthorizedAccessException("invalid email or password");

        if (user.AuthProvider != "local" || user.PasswordHash == null)
            throw new UnauthorizedAccessException("this account uses a different login method");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("invalid email or password");

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(string idToken)
    {
        var payload = await _googleTokenValidator.ValidateAsync(idToken);

        if (payload == null || string.IsNullOrWhiteSpace(payload.Email))
            throw new UnauthorizedAccessException("invalid google id token");

        return await FindOrCreateGoogleUserAsync(payload.Email);
    }

    // used only in development/testing — bypasses google signature check
    public async Task<AuthResponseDto> GoogleLoginByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("email is required");

        return await FindOrCreateGoogleUserAsync(email.ToLower().Trim());
    }

    private async Task<AuthResponseDto> FindOrCreateGoogleUserAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
        {
            user = new AppUser
            {
                Email = email,
                PasswordHash = null,
                Role = "Resident",
                AuthProvider = "google",
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        return new AuthResponseDto
        {
            Token = GenerateJwtToken(user),
            Email = user.Email,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(AppUser user)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("jwt key is not configured");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
