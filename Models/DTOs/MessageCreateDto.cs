using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class MessageCreateDto
{
    [Required, MaxLength(2000)]
    public string Content { get; set; }
}
