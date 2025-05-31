namespace MyChat.Models.DTOs.Messages;

public class ConversationDto
{
    public int Id { get; set; }
    public ConversationType Type { get; set; }
    public string Name { get; set; } = string.Empty; // Contact username or group name
    public MessageDto? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime LastActivity { get; set; }
    
    // For direct messages
    public int? ContactId { get; set; }
    
    // For groups
    public int? GroupId { get; set; }
    public int? MemberCount { get; set; }
}

public enum ConversationType
{
    Direct,
    Group
}
