using MyChat.Entities;
using MyChat.Models.DTOs.Auth;

namespace MyChat.Models.DTOs.Messages;

public class MessageDto
{
    public int Id { get; set; }
    public UserDto Sender { get; set; } = null!;
    public string Content { get; set; } = string.Empty;
    public MessageType Type { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsOwn { get; set; } // True if current user sent this message
    
    // Group context
    public int? GroupId { get; set; }
    public string? GroupName { get; set; }
    
    // Direct message context
    public int? RecipientId { get; set; }
    public string? RecipientUsername { get; set; }
}
