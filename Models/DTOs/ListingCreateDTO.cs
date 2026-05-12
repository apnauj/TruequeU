using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TruequeU.Enums;

namespace TruequeU.Models.DTOs
{
    public class ListingCreateDTO
    {
        [Required, MaxLength(100) ]
        public string Title { get; set;}

        [Required, MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required, Range(0.0, double.MaxValue)]

        public decimal Price { get; set; }


        [Required]

        public Category Category { get; set; }

        [Required]

        public ItemCondition Condition { get; set; }


        [Required, MaxLength(100)]
        public string CampusLocation { get; set; } = string.Empty; 














    }
}
