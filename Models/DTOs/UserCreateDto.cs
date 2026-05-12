using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class UserCreateDto
{
    [Required, MaxLength(50)]
    public string Username { get; set; }
    
    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; set; }
    
    [Required, PasswordPropertyText]
    public string PasswordHash { get; set; }
    
    [MaxLength(100)]
    public string? FullName { get; set; }
}