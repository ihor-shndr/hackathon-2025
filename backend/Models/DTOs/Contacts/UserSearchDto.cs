namespace MyChat.Models.DTOs.Contacts;

public class UserSearchDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public bool IsContact { get; set; }
    public bool HasPendingInvitation { get; set; }
    public bool IsSentInvitation { get; set; }
}
