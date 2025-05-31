using MyChat.Models.DTOs.Auth;

namespace MyChat.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<bool> UsernameExistsAsync(string username);
}
