using System.ComponentModel.DataAnnotations;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public class ListingUpdateDTO
    {
        [MaxLength(100)]
        public string? Title { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; } = string.Empty;

        [Range(0.0, double.MaxValue)]
        public decimal? Price { get; set; }

        
        public Category? Category { get; set; }

        
        public ItemCondition? Condition { get; set; }

        
        public string? CampusLocation { get; set; }
    }
}
