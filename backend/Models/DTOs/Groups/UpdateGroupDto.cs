using System.ComponentModel.DataAnnotations;

namespace MyChat.Models.DTOs.Groups;

public class UpdateGroupDto
{
    [MaxLength(100)]
    public string? Name { get; set; }
    
    [MaxLength(500)]
    public string? Description { get; set; }
}
