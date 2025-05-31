namespace MyChat.Models.DTOs.Groups;

public class GroupMemberDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsOwner { get; set; }
}
