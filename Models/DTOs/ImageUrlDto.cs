using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class ImageUrlDto
{
    [Required]
    [Url]
    public string Url { get; set; } = string.Empty;
}
