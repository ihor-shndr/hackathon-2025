using System.ComponentModel.DataAnnotations;

namespace MyChat.Entities;

public class Group
{
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
