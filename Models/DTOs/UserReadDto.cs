using System;

namespace TruequeU.Models.DTOs;

public record UserReadDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public double Rating { get; set; }
}