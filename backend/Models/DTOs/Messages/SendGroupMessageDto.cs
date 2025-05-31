using System.ComponentModel.DataAnnotations;
using MyChat.Entities;

namespace MyChat.Models.DTOs.Messages;

public class SendGroupMessageDto
{
    [Required]
    public int GroupId { get; set; }
    
    [Required]
    [MaxLength(4000)]
    public string Content { get; set; } = string.Empty;
    
    public MessageType Type { get; set; } = MessageType.Text;
    
    [MaxLength(500)]
    public string? AttachmentUrl { get; set; }
}
