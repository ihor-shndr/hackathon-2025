using MyChat.Entities;
using MyChat.Models.DTOs.Auth;

namespace MyChat.Models.DTOs.Contacts;

public class ContactDto
{
    public int Id { get; set; }
    public UserDto User { get; set; } = null!;
    public ContactStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? Message { get; set; }
}
