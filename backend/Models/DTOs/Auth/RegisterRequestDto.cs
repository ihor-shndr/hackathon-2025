using System.ComponentModel.DataAnnotations;

namespace MyChat.Models.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    [StringLength(255, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [StringLength(255, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    [Compare("Password", ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
