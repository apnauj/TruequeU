using System.ComponentModel.DataAnnotations;

namespace TruequeU.Configuration;

public class JwtSettings
{
    public const string SectionName = "Jwt";

    [Required(ErrorMessage = "JWT Key is required.")]
    [MinLength(32, ErrorMessage = "JWT Key must be at least 32 characters long.")]
    public string Key { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required.")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required.")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "JWT expiration must be between 1 and 1440 minutes.")]
    public int ExpireMinutes { get; set; } = 60;
}
