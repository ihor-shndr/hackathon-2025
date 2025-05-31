using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Entities;
using MyChat.Models.DTOs.Auth;

namespace MyChat.Services;

public class AuthService : IAuthService
{
    private readonly ChatDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(ChatDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        // Check if username already exists
        if (await UsernameExistsAsync(request.Username))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Create new user with raw password (as requested)
        var user = new User
        {
            Username = request.Username,
            Password = request.Password, // Raw password storage
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        // Find user by username with raw password comparison
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password);

        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        // Update last seen
        user.LastSeenAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = _jwtService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            }
        };
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        return await _context.Users.AnyAsync(u => u.Username == username);
    }
}
