using System;
using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class ReportCreateDto
{
    [Required]
    public Guid ReportedUserId { get; set; }

    public Guid? ReportedListingId { get; set; }

    [Required, MaxLength(100)]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(2000)]
    public string Comment { get; set; } = string.Empty;
}
