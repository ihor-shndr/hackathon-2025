using System.ComponentModel.DataAnnotations;

namespace MyChat.Entities;

public class User
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Username { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string Password { get; set; } = string.Empty; // Raw password as requested
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastSeenAt { get; set; }
}
