using System.ComponentModel.DataAnnotations;

namespace TruequeU.Models.DTOs;

public class ResolveReportDto
{
    [Required, MaxLength(500)]
    public string ResolutionNote { get; set; } = string.Empty;
}
