using System;
using System.ComponentModel.DataAnnotations;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public class ReportUpdateDto
    {
        [Required]
        public ReportStatus Status { get; set; }
        
        public Guid? ResolvedByUserId { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
        
        [MaxLength(500)]
        public string? ResolutionNote { get; set; }
    }
}