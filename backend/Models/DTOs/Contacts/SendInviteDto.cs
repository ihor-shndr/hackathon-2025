using System.ComponentModel.DataAnnotations;

namespace MyChat.Models.DTOs.Contacts;

public class SendInviteDto
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;
    
    [StringLength(500)]
    public string? Message { get; set; }
}
