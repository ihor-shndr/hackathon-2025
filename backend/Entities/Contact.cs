using System.ComponentModel.DataAnnotations;

namespace MyChat.Entities;

public class Contact
{
    public int Id { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    public int ContactUserId { get; set; }
    
    [Required]
    public ContactStatus Status { get; set; } = ContactStatus.Pending;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? AcceptedAt { get; set; }
    
    public string? Message { get; set; }
    
    // Navigation properties
    public User User { get; set; } = null!;
    public User ContactUser { get; set; } = null!;
}

public enum ContactStatus
{
    Pending = 0,
    Accepted = 1,
    Blocked = 2
}
