using System.ComponentModel.DataAnnotations;

namespace MyChat.Entities;

public class MessageReaction
{
    public int Id { get; set; }
    
    public int MessageId { get; set; }
    public Message Message { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    [Required]
    [MaxLength(10)]
    public string Emoji { get; set; } = string.Empty; // 👍, ❤️, 😂, etc.
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
