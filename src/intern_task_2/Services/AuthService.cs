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

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
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

        var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
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

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
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

        var passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordValid)
            throw new UnauthorizedAccessException("invalid email or password");

        var token = GenerateJwtToken(user);

        return new AuthResponseDto
        {
            Token = token,
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
