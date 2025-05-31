using System.ComponentModel.DataAnnotations;

namespace MyChat.Models.DTOs.Groups;

public class CreateGroupDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    [MaxLength(500)]
    public string? Description { get; set; }
    
    public List<int>? InitialMemberIds { get; set; }
}
