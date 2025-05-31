using System.ComponentModel.DataAnnotations;

namespace MyChat.Entities;

public class Message
{
    public int Id { get; set; }
    
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;
    
    // Either GroupId OR RecipientId will be set, not both
    public int? GroupId { get; set; }
    public Group? Group { get; set; }
    
    public int? RecipientId { get; set; }
    public User? Recipient { get; set; }
    
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
    
    public MessageType Type { get; set; } = MessageType.Text;
    
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
    
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; } = false;
    
    // Future: Message reactions
    public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
}

public enum MessageType
{
    Text,
    Image,
    File
}
