using System;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs;

public record ReportReadDto
{
    public Guid Id { get; set; }
    public Guid ReporterId { get; set; }
    public string ReporterName { get; set; } = string.Empty;
    public Guid ReportedUserId { get; set; }
    public string ReportedUserName { get; set; } = string.Empty;
    public Guid? ReportedListingId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public string? ResolvedByUserName { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? ResolutionNote { get; set; }
}
