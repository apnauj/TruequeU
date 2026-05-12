using System;
using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class ConversationCreateDto
{
    [Required]
    public Guid ListingId { get; set; }

    [Required, MaxLength(2000)]
    public string Content { get; set; }
}
