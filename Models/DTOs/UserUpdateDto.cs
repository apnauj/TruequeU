using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class UserUpdateDto
{
    [MaxLength(50)]
    public string? Username { get; set; }

    [MaxLength(100)]
    public string? FullName { get; set; }

    [MaxLength(100)]
    public string? Program { get; set; } 

    [MaxLength(500)]
    public string? Bio { get; set; }
    
    [Url] 
    public string? AvatarUrl { get; set; }
    
    [EmailAddress, MaxLength(255)]
    public string? Email { get; set; }
}