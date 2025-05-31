using System.ComponentModel.DataAnnotations;

namespace MyChat.Models.DTOs.Contacts;

public class RespondToInviteDto
{
    [Required]
    public bool Accept { get; set; }
}
