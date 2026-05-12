using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class UserUpdateDto
{
    [MaxLength(100)]
    public string? FullName { get; init; }

    [MaxLength(100)]
    public string? Program { get; init; }

    [MaxLength(500)]
    public string? Bio { get; init; }

    [Url]
    public string? AvatarUrl { get; init; }
}