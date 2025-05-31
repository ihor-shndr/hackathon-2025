using MyChat.Models.DTOs.Auth;

namespace MyChat.Models.DTOs.Contacts;

public class ContactInvitationDto
{
    public int Id { get; set; }
    public UserDto FromUser { get; set; } = null!;
    public UserDto ToUser { get; set; } = null!;
    public string? Message { get; set; }
    public DateTime SentAt { get; set; }
}
