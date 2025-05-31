using MyChat.Models.DTOs.Auth;
using MyChat.Models.DTOs.Messages;

namespace MyChat.Models.DTOs.Groups;

public class GroupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public UserDto Owner { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public MessageDto? LastMessage { get; set; }
}
