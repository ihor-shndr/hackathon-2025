namespace MyChat.Models.DTOs.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
