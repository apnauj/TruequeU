using System;
using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class RegisterRequestDto
{
    [Required, MaxLength(50)]
    public string UserName { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(255)]
    public string Email { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string Password { get; init; } = string.Empty;

    [MaxLength(100)]
    public string? FullName { get; init; }
}

public class LoginRequestDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; init; } = string.Empty;
}

public class AuthResponseDto
{
    public string Token { get; init; } = string.Empty;
    public DateTime Expiration { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public class ResetPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Token { get; init; } = string.Empty;

    [Required, DataType(DataType.Password), MinLength(6)]
    public string NewPassword { get; init; } = string.Empty;
}
