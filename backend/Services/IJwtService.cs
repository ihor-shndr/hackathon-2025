using System.Security.Claims;
using MyChat.Entities;

namespace MyChat.Services;

public interface IJwtService
{
    string GenerateToken(User user);
    ClaimsPrincipal? ValidateToken(string token);
}
