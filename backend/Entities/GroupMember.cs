namespace MyChat.Entities;

public class GroupMember
{
    public int GroupId { get; set; }
    public Group Group { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true; // For soft leave functionality
}
