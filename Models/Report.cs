using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TruequeU.Enums;

namespace TruequeU.Models;

public class Report
{
    [Key]
    public Guid Id { get; private set; }
    
    [Required]
    public Guid ReporterId { get; set; }
    
    [ForeignKey(nameof(ReporterId))]
    public User Reporter { get; set; } = null!;
    
    [Required]
    public Guid ReportedUserId { get; set; }
    
    [ForeignKey(nameof(ReportedUserId))]
    public User ReportedUser { get; set; } = null!; 
    
    public Guid? ReportedListingId { get; set; }
    
    [ForeignKey(nameof(ReportedListingId))]
    public Listing? ReportedListing { get; set; }
    
    [Required, MaxLength(100)]
    public string Reason { get; set; }
    
    [Required, MaxLength(2000)]
    public string Comment { get; set; }
    
    public ReportStatus Status { get; set; }

    public DateTime CreatedAt { get; private set; }

    public Guid? ResolvedByUserId { get; set; }

    [ForeignKey(nameof(ResolvedByUserId))]
    public User? ResolvedByUser { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [MaxLength(500)]
    public string? ResolutionNote { get; set; }
    
    private Report() { }
    
    public Report(
        Guid reporterId, 
        Guid reportedUserId, 
        string reason, 
        string comment, 
        Guid? reportedListingId = null) 
    {
        if (reporterId == reportedUserId)
            throw new ArgumentException("No puedes reportarte a ti mismo.");

        Id = Guid.NewGuid();
        ReporterId = reporterId;
        ReportedUserId = reportedUserId;
        ReportedListingId = reportedListingId;
        Reason = reason;
        Comment = comment;
        
        // Valores automáticos
        Status = ReportStatus.Open;
        CreatedAt = DateTime.UtcNow;
    }
}